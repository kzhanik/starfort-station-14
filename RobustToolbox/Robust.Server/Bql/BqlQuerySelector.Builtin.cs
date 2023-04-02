using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Robust.Server.Bql
{
    [RegisterBqlQuerySelector]
    public sealed class WithQuerySelector : BqlQuerySelector
    {
        public override string Token => "with";

        public override QuerySelectorArgument[] Arguments => new []{ QuerySelectorArgument.Component };

        public override IEnumerable<EntityUid> DoSelection(IEnumerable<EntityUid> input, IReadOnlyList<object> arguments, bool isInverted, IEntityManager entityManager)
        {
            var comp = (Type) arguments[0];
            return input.Where(x => entityManager.HasComponent(x, comp) ^ isInverted);
        }

        public override IEnumerable<EntityUid> DoInitialSelection(IReadOnlyList<object> arguments, bool isInverted, IEntityManager entityManager)
        {
            if (isInverted)
            {
                return base.DoInitialSelection(arguments, isInverted, entityManager);
            }

            return entityManager.GetAllComponents((Type) arguments[0])
                .Select(x => x.Owner);
        }
    }

    [RegisterBqlQuerySelector]
    public sealed class NamedQuerySelector : BqlQuerySelector
    {
        public override string Token => "named";

        public override QuerySelectorArgument[] Arguments => new []{ QuerySelectorArgument.String };

        public override IEnumerable<EntityUid> DoSelection(IEnumerable<EntityUid> input, IReadOnlyList<object> arguments, bool isInverted, IEntityManager entityManager)
        {
            var r = new Regex("^" + (string) arguments[0] + "$");
            return input.Where(e =>
            {
                if (entityManager.TryGetComponent<MetaDataComponent>(e, out var metaDataComponent))
                    return r.IsMatch(metaDataComponent.EntityName) ^ isInverted;
                return isInverted;
            });
        }
    }

    [RegisterBqlQuerySelector]
    public sealed class ParentedToQuerySelector : BqlQuerySelector
    {
        public override string Token => "parentedto";

        public override QuerySelectorArgument[] Arguments => new []{ QuerySelectorArgument.EntityId };

        public override IEnumerable<EntityUid> DoSelection(IEnumerable<EntityUid> input, IReadOnlyList<object> arguments, bool isInverted, IEntityManager entityManager)
        {
            var uid = (EntityUid) arguments[0];
            return input.Where(e => (entityManager.TryGetComponent<TransformComponent>(e, out var transform) &&
                                     transform.ParentUid == uid) ^ isInverted);
        }
    }

    [RegisterBqlQuerySelector]
    public sealed class RecursiveParentedToQuerySelector : BqlQuerySelector
    {
        public override string Token => "rparentedto";

        public override QuerySelectorArgument[] Arguments => new []{ QuerySelectorArgument.EntityId };

        public override IEnumerable<EntityUid> DoSelection(IEnumerable<EntityUid> input, IReadOnlyList<object> arguments, bool isInverted, IEntityManager entityManager)
        {
            var uid = (EntityUid) arguments[0];
            return input.Where(e =>
            {
                if (!entityManager.TryGetComponent<TransformComponent>(e, out var transform))
                    return isInverted;
                while (transform.ParentUid != EntityUid.Invalid)
                {
                    if ((transform.ParentUid == uid) ^ isInverted)
                        return true;
                    if (!entityManager.TryGetComponent(transform.ParentUid, out transform))
                        return false;
                }

                return false;
            });
        }
    }

    [RegisterBqlQuerySelector]
    public sealed class ChildrenQuerySelector : BqlQuerySelector
    {
        public override string Token => "children";

        public override QuerySelectorArgument[] Arguments => Array.Empty<QuerySelectorArgument>();

        public override IEnumerable<EntityUid> DoSelection(IEnumerable<EntityUid> input, IReadOnlyList<object> arguments, bool isInverted, IEntityManager entityManager)
        {
            foreach (var uid in input)
            {
                if (!entityManager.TryGetComponent(uid, out TransformComponent? xform)) continue;

                foreach (var child in xform.ChildEntities)
                {
                    yield return child;
                }
            }
        }
    }

    [RegisterBqlQuerySelector]
    public sealed class RecursiveChildrenQuerySelector : BqlQuerySelector
    {
        public override string Token => "rchildren";

        public override QuerySelectorArgument[] Arguments => Array.Empty<QuerySelectorArgument>();

        public override IEnumerable<EntityUid> DoSelection(IEnumerable<EntityUid> input, IReadOnlyList<object> arguments,
            bool isInverted, IEntityManager entityManager)
        {
            IEnumerable<EntityUid> toSearch = input;

            while (true)
            {
                // TODO: Reduce LINQ chaining
                var doing = toSearch.Where(entityManager.HasComponent<TransformComponent>).Select(entityManager.GetComponent<TransformComponent>).ToArray();
                var search = doing.SelectMany(x => x.ChildEntities);
                if (!search.Any())
                    break;
                toSearch = doing.SelectMany(x => x.ChildEntities).Where(x => x != EntityUid.Invalid);

                foreach (var xform in doing)
                {
                    foreach (var uid in xform.ChildEntities)
                    {
                        // This should never happen anyway
                        if (uid == EntityUid.Invalid) continue;
                        yield return uid;
                    }
                }
            }
        }
    }

    [RegisterBqlQuerySelector]
    public sealed class ParentQuerySelector : BqlQuerySelector
    {
        public override string Token => "parent";

        public override QuerySelectorArgument[] Arguments => Array.Empty<QuerySelectorArgument>();

        public override IEnumerable<EntityUid> DoSelection(IEnumerable<EntityUid> input, IReadOnlyList<object> arguments, bool isInverted, IEntityManager entityManager)
        {
            return input.Where(entityManager.HasComponent<TransformComponent>)
                .Distinct();
        }

        public override IEnumerable<EntityUid> DoInitialSelection(IReadOnlyList<object> arguments, bool isInverted, IEntityManager entityManager)
        {
            return DoSelection(
                Query(entityManager.AllEntityQueryEnumerator<TransformComponent>()),
                arguments,
                isInverted, entityManager);

            IEnumerable<EntityUid> Query(AllEntityQueryEnumerator<TransformComponent> enumerator)
            {
                while (enumerator.MoveNext(out var entityUid, out _))
                {
                    yield return entityUid;
                }
            }
        }
    }

    [RegisterBqlQuerySelector]
    public sealed class AboveQuerySelector : BqlQuerySelector
    {
        public override string Token => "above";

        public override QuerySelectorArgument[] Arguments => new [] { QuerySelectorArgument.String };

        public override IEnumerable<EntityUid> DoSelection(IEnumerable<EntityUid> input, IReadOnlyList<object> arguments, bool isInverted, IEntityManager entityManager)
        {
            var tileDefinitionManager = IoCManager.Resolve<ITileDefinitionManager>();
            var tileTy = tileDefinitionManager[(string) arguments[0]];

            var map = IoCManager.Resolve<IMapManager>();
            if (tileTy.TileId == 0)
            {
                return input.Where(e => entityManager.TryGetComponent<TransformComponent>(e, out var transform) && (transform.GridUid is null) ^ isInverted);
            }
            else
            {
                return input.Where(e =>
                {
                    if (!entityManager.TryGetComponent<TransformComponent>(e, out var transform)) return isInverted;

                    if (!map.TryGetGrid(transform.GridUid, out var grid))
                        return isInverted;

                    return (grid.GetTileRef(transform.Coordinates).Tile.TypeId == tileTy.TileId) ^ isInverted;

                });
            }
        }
    }

    [RegisterBqlQuerySelector]
    // ReSharper disable once InconsistentNaming the name is correct shut up
    public sealed class OnGridQuerySelector : BqlQuerySelector
    {
        public override string Token => "ongrid";

        public override QuerySelectorArgument[] Arguments => new [] { QuerySelectorArgument.Integer };

        public override IEnumerable<EntityUid> DoSelection(IEnumerable<EntityUid> input, IReadOnlyList<object> arguments, bool isInverted, IEntityManager entityManager)
        {
            // TODO: Probably easier and significantly faster to just iterate the grid's children.
            var grid = new EntityUid((int) arguments[0]);
            return input.Where(e => (entityManager.TryGetComponent<TransformComponent>(e, out var transform) && transform.GridUid == grid) ^ isInverted);
        }
    }

    [RegisterBqlQuerySelector]
    // ReSharper disable once InconsistentNaming the name is correct shut up
    public sealed class OnMapQuerySelector : BqlQuerySelector
    {
        public override string Token => "onmap";

        public override QuerySelectorArgument[] Arguments => new [] { QuerySelectorArgument.Integer };

        public override IEnumerable<EntityUid> DoSelection(IEnumerable<EntityUid> input, IReadOnlyList<object> arguments, bool isInverted, IEntityManager entityManager)
        {
            // TODO: Just use EntityLookup GetEntitiesInMap
            var map = new MapId((int) arguments[0]);
            return input.Where(e => (entityManager.TryGetComponent<TransformComponent>(e, out var transform) && transform.MapID == map) ^ isInverted);
        }
    }

    [RegisterBqlQuerySelector]
    public sealed class PrototypedQuerySelector : BqlQuerySelector
    {
        public override string Token => "prototyped";

        public override QuerySelectorArgument[] Arguments => new []{ QuerySelectorArgument.String };

        public override IEnumerable<EntityUid> DoSelection(IEnumerable<EntityUid> input, IReadOnlyList<object> arguments, bool isInverted, IEntityManager entityManager)
        {
            var name = (string) arguments[0];
            return input.Where(e => (entityManager.TryGetComponent<MetaDataComponent>(e, out var metaData) && metaData.EntityPrototype?.ID == name) ^ isInverted);
        }
    }

    [RegisterBqlQuerySelector]
    public sealed class RecursivePrototypedQuerySelector : BqlQuerySelector
    {
        public override string Token => "rprototyped";

        public override QuerySelectorArgument[] Arguments => new []{ QuerySelectorArgument.String };

        public override IEnumerable<EntityUid> DoSelection(IEnumerable<EntityUid> input, IReadOnlyList<object> arguments, bool isInverted, IEntityManager entityManager)
        {
            var name = (string) arguments[0];
            return input.Where(e =>
            {
                if (!entityManager.TryGetComponent<MetaDataComponent>(e, out var metaData))
                    return isInverted;
                if ((metaData.EntityPrototype?.ID == name) ^ isInverted)
                    return true;

                var prototypeManager = IoCManager.Resolve<IPrototypeManager>();

                return metaData.EntityPrototype != null && prototypeManager.EnumerateParents<EntityPrototype>(metaData.EntityPrototype.ID).Any(x => x.Name == name) ^ isInverted;
            });
        }
    }

    [RegisterBqlQuerySelector]
    public sealed class SelectQuerySelector : BqlQuerySelector
    {
        public override string Token => "select";

        public override QuerySelectorArgument[] Arguments => new []{ QuerySelectorArgument.Integer | QuerySelectorArgument.Percentage };

        public override IEnumerable<EntityUid> DoSelection(IEnumerable<EntityUid> input, IReadOnlyList<object> arguments, bool isInverted, IEntityManager entityManager)
        {
            if (arguments[0] is int)
            {
                var inp = input.OrderBy(_ => Guid.NewGuid()).ToArray();
                var taken = (int) arguments[0];

                if (isInverted)
                    taken = Math.Max(0, inp.Length - taken);

                return inp.Take(taken);
            }

            var enumerable = input.OrderBy(_ => Guid.NewGuid()).ToArray();
            var amount = isInverted
                ? (int) Math.Floor(enumerable.Length * Math.Clamp(1 - (double) arguments[0], 0, 1))
                : (int) Math.Floor(enumerable.Length * Math.Clamp((double) arguments[0], 0, 1));
            return enumerable.Take(amount);
        }
    }

    [RegisterBqlQuerySelector]
    public sealed class NearQuerySelector : BqlQuerySelector
    {
        public override string Token => "near";

        public override QuerySelectorArgument[] Arguments => new []{ QuerySelectorArgument.Float };

        public override IEnumerable<EntityUid> DoSelection(IEnumerable<EntityUid> input, IReadOnlyList<object> arguments, bool isInverted, IEntityManager entityManager)
        {
            var radius = (float)(double)arguments[0];
            var entityLookup = entityManager.System<EntityLookupSystem>();
            var xformQuery = entityManager.GetEntityQuery<TransformComponent>();
            var distinct = new HashSet<EntityUid>();

            foreach (var uid in input)
            {
                foreach (var near in entityLookup.GetEntitiesInRange(xformQuery.GetComponent(uid).Coordinates,
                             radius))
                {
                    if (!distinct.Add(near)) continue;
                    yield return near;
                }
            }

            //BUG: GetEntitiesInRange effectively uses manhattan distance. This is not intended, near is supposed to be circular.
        }
    }

    [RegisterBqlQuerySelector]
    // ReSharper disable once InconsistentNaming the name is correct shut up
    public sealed class AnchoredQuerySelector : BqlQuerySelector
    {
        public override string Token => "anchored";

        public override QuerySelectorArgument[] Arguments => Array.Empty<QuerySelectorArgument>();

        public override IEnumerable<EntityUid> DoSelection(IEnumerable<EntityUid> input, IReadOnlyList<object> arguments, bool isInverted, IEntityManager entityManager)
        {
            return input.Where(e => (entityManager.TryGetComponent<TransformComponent>(e, out var transform) && transform.Anchored) ^ isInverted);
        }
    }
}
