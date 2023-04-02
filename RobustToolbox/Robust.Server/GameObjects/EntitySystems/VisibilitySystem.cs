using System;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;

namespace Robust.Server.GameObjects
{
    public sealed class VisibilitySystem : EntitySystem
    {
        [Dependency] private readonly MetaDataSystem _metaSys = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<EntParentChangedMessage>(OnParentChange);
            EntityManager.EntityInitialized += OnEntityInit;
        }

        public override void Shutdown()
        {
            base.Shutdown();
            EntityManager.EntityInitialized -= OnEntityInit;
        }

        public void AddLayer(EntityUid uid, VisibilityComponent component, int layer, bool refresh = true)
        {
            if ((layer & component.Layer) == layer)
                return;

            component.Layer |= layer;

            if (refresh)
                RefreshVisibility(uid, visibilityComponent: component);
        }

        [Obsolete("Use overload that takes an EntityUid instead")]
        public void AddLayer(VisibilityComponent component, int layer, bool refresh = true)
        {
            AddLayer(component.Owner, component, layer, refresh);
        }

        public void RemoveLayer(EntityUid uid, VisibilityComponent component, int layer, bool refresh = true)
        {
            if ((layer & component.Layer) != layer)
                return;

            component.Layer &= ~layer;

            if (refresh)
                RefreshVisibility(uid, visibilityComponent: component);
        }

        [Obsolete("Use overload that takes an EntityUid instead")]
        public void RemoveLayer(VisibilityComponent component, int layer, bool refresh = true)
        {
            RemoveLayer(component.Owner, component, layer, refresh);
        }

        public void SetLayer(EntityUid uid, VisibilityComponent component, int layer, bool refresh = true)
        {
            if (component.Layer == layer)
                return;

            component.Layer = layer;

            if (refresh)
                RefreshVisibility(uid, visibilityComponent: component);
        }

        [Obsolete("Use overload that takes an EntityUid instead")]
        public void SetLayer(VisibilityComponent component, int layer, bool refresh = true)
        {
            SetLayer(component.Owner, component, layer, refresh);
        }

        private void OnParentChange(ref EntParentChangedMessage ev)
        {
            RefreshVisibility(ev.Entity);
        }

        private void OnEntityInit(EntityUid uid)
        {
            RefreshVisibility(uid);
        }

        public void RefreshVisibility(EntityUid uid, MetaDataComponent? metaDataComponent = null, VisibilityComponent? visibilityComponent = null)
        {
            if (Resolve(uid, ref metaDataComponent, false))
                _metaSys.SetVisibilityMask(uid, GetVisibilityMask(uid, visibilityComponent), metaDataComponent);
        }

        [Obsolete("Use overload that takes an EntityUid instead")]
        public void RefreshVisibility(VisibilityComponent visibilityComponent)
        {
            RefreshVisibility(visibilityComponent.Owner, null, visibilityComponent);
        }

        private int GetVisibilityMask(EntityUid uid, VisibilityComponent? visibilityComponent = null, TransformComponent? xform = null)
        {
            int visMask = 1; // apparently some content expects everything to have the first bit/flag set to true.
            if (Resolve(uid, ref visibilityComponent, false))
                visMask |= visibilityComponent.Layer;

            // Include parent vis masks
            if (Resolve(uid, ref xform) && xform.ParentUid.IsValid())
                visMask |= GetVisibilityMask(xform.ParentUid);

            return visMask;
        }
    }
}
