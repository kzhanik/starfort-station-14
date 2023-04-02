using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Linguini.Bundle;
using Linguini.Bundle.Builder;
using Linguini.Bundle.Errors;
using Linguini.Shared.Types.Bundle;
using Linguini.Syntax.Ast;
using Linguini.Syntax.Parser;
using Linguini.Syntax.Parser.Error;
using Robust.Shared.ContentPack;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Log;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Robust.Shared.Localization
{
    internal sealed partial class LocalizationManager : ILocalizationManagerInternal, IPostInjectInit
    {
        [Dependency] private readonly IResourceManager _res = default!;
        [Dependency] private readonly ILogManager _log = default!;
        [Dependency] private readonly IPrototypeManager _prototype = default!;
        [Dependency] private readonly IEntityManager _entMan = default!;

        private ISawmill _logSawmill = default!;
        private readonly Dictionary<CultureInfo, FluentBundle> _contexts = new();

        private CultureInfo? _defaultCulture;
        private CultureInfo? _fallbackCulture;

        void IPostInjectInit.PostInject()
        {
            _logSawmill = _log.GetSawmill("loc");
            _prototype.PrototypesReloaded += OnPrototypesReloaded;
        }

        public string GetString(string messageId)
        {
            if (_defaultCulture == null)
                return messageId;

            if (!TryGetString(messageId, out var msg))
            {
                _logSawmill.Debug("Unknown messageId ({culture}): {messageId}", _defaultCulture.Name, messageId);
                msg = messageId;
            }

            return msg;
        }


        public string GetString(string messageId, params (string, object)[] args0)
        {
            if (_defaultCulture == null)
                return messageId;

            if (!TryGetString(messageId, out var msg, args0))
            {
                _logSawmill.Debug("Unknown messageId ({culture}): {messageId}", _defaultCulture.Name, messageId);
                msg = messageId;
            }

            return msg;
        }

        public bool TryGetString(string messageId, [NotNullWhen(true)] out string? value)
        {
            return TryGetString(messageId, out value, null);
        }

        public bool TryGetString(string messageId, [NotNullWhen(true)] out string? value,
            params (string, object)[]? keyArgs)
        {
            if (!HasMessage(messageId, out var bundle))
            {
                value = null;
                return false;
            }

            var context = new LocContext(bundle);
            var args = new Dictionary<string, IFluentType>();
            if (keyArgs != null)
            {
                foreach (var (k, v) in keyArgs)
                {
                    args.Add(k, v.FluentFromObject(context));
                }
            }

            try
            {
                var result = bundle.TryGetAttrMsg(messageId, args, out var errs, out value);
                foreach (var err in errs)
                {
                    _logSawmill.Error("{culture}/{messageId}: {error}", _defaultCulture!.Name, messageId, err);
                }

                return result;
            }
            catch (Exception e)
            {
                _logSawmill.Error("{culture}/{messageId}: {exception}", _defaultCulture!.Name, messageId, e);
                value = null;
                return false;
            }
        }

        private bool HasMessage(
            string messageId,
            [NotNullWhen(true)] out FluentBundle? bundle)
        {
            foreach (var culture in new[] { _defaultCulture, _fallbackCulture })
            {
                if (culture != null && _contexts.TryGetValue(culture, out bundle))
                {
                    if (messageId.Contains('.'))
                    {
                        var split = messageId.Split('.');
                        if (bundle.HasMessage(split[0]))
                            return true;
                    }
                    if (bundle.HasMessage(messageId))
                        return true;
                }
            }

            bundle = null;
            return false;
        }

        private bool TryGetMessage(
            string messageId,
            [NotNullWhen(true)] out FluentBundle? bundle,
            [NotNullWhen(true)] out AstMessage? message)
        {
            foreach (var culture in new[] { _defaultCulture, _fallbackCulture })
            {
                if (culture != null && _contexts.TryGetValue(culture, out bundle))
                {
                    if (bundle.TryGetAstMessage(messageId, out message))
                        return true;
                }
            }

            bundle = null;
            message = null;
            return false;
        }

        public void ReloadLocalizations()
        {
            foreach (var (culture, context) in _contexts.ToArray())
            {
                _loadData(_res, culture, context);
            }

            FlushEntityCache();
        }

        public CultureInfo? DefaultCulture
        {
            get => _defaultCulture;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (!_contexts.ContainsKey(value))
                {
                    throw new ArgumentException("That culture is not yet loaded and cannot be used.", nameof(value));
                }

                _defaultCulture = value;
                CultureInfo.CurrentCulture = value;
                CultureInfo.CurrentUICulture = value;
            }
        }

        public void LoadCulture(CultureInfo culture)
        {
            var bundle = LinguiniBuilder.Builder()
                .CultureInfo(culture)
                .SkipResources()
                .SetUseIsolating(false)
                .UseConcurrent()
                .UncheckedBuild();

            _contexts.Add(culture, bundle);
            AddBuiltInFunctions(bundle);

            _loadData(_res, culture, bundle);
            DefaultCulture ??= culture;
        }

        public void SetFallbackCluture(CultureInfo culture)
        {
            if (!_contexts.ContainsKey(culture))
            {
                throw new ArgumentException("That culture is not loaded.", nameof(culture));
            }

            _fallbackCulture = culture;
        }

        public void AddLoadedToStringSerializer(IRobustMappedStringSerializer serializer)
        {
            /*
             * TODO: need to expose Messages on MessageContext in Fluent.NET
            serializer.AddStrings(StringIterator());

            IEnumerable<string> StringIterator()
            {
                foreach (var context in _contexts.Values)
                {
                    foreach (var (key, translations) in _context)
                    {
                        yield return key;

                        foreach (var t in translations)
                        {
                            yield return t;
                        }
                    }
                }
            }
            */
        }

        private void _loadData(IResourceManager resourceManager, CultureInfo culture, FluentBundle context)
        {
            // Load data from .ftl files.
            // Data is loaded from /Locale/<language-code>/*

            var root = new ResourcePath($"/Locale/{culture.Name}/");

            var files = resourceManager.ContentFindFiles(root)
                .Where(c => c.Filename.EndsWith(".ftl", StringComparison.InvariantCultureIgnoreCase))
                .ToArray();

            var resources = files.AsParallel().Select(path =>
            {
                using var fileStream = resourceManager.ContentFileRead(path);
                using var reader = new StreamReader(fileStream, EncodingHelpers.UTF8);

                var parser = new LinguiniParser(reader);
                var resource = parser.Parse();
                return (path, resource, parser.GetReadonlyData);
            });

            foreach (var (path, resource, data) in resources)
            {
                var errors = resource.Errors;
                context.AddResourceOverriding(resource);
                WriteWarningForErrs(path, errors, data);
            }
        }

        private void WriteWarningForErrs(ResourcePath path, List<ParseError> errs, ReadOnlyMemory<char> resource)
        {
            foreach (var err in errs)
            {
                _logSawmill.Warning("{path}:\n{exception}", path, err.FormatCompileErrors(resource));
            }
        }

        private void WriteWarningForErrs(IList<FluentError> errs, string locId)
        {
            foreach (var err in errs)
            {
                _logSawmill.Warning("Error extracting `{locId}`\n{e1}", locId, err);
            }
        }
    }
}
