using System;
using System.Globalization;
using System.Linq;
using Robust.Shared.GameObjects;
using Robust.Shared.Log;
using Robust.Shared.Map.Components;

namespace Robust.Shared.Map
{
    internal partial class MapManager
    {
        /// <inheritdoc />
        public void SetMapPaused(MapId mapId, bool paused)
        {
            if(mapId == MapId.Nullspace)
                return;

            if(!MapExists(mapId))
                throw new ArgumentException("That map does not exist.");

            var mapUid = GetMapEntityId(mapId);
            var mapComp = EntityManager.GetComponent<MapComponent>(mapUid);

            if (mapComp.MapPaused == paused)
                return;

            mapComp.MapPaused = paused;
            EntityManager.Dirty(mapComp);

            var xformQuery = EntityManager.GetEntityQuery<TransformComponent>();
            var metaQuery = EntityManager.GetEntityQuery<MetaDataComponent>();
            var metaSystem = EntityManager.EntitySysManager.GetEntitySystem<MetaDataSystem>();

            RecursiveSetPaused(mapUid, paused, in xformQuery, in metaQuery, in metaSystem);
        }

        private static void RecursiveSetPaused(EntityUid entity, bool paused,
            in EntityQuery<TransformComponent> xformQuery,
            in EntityQuery<MetaDataComponent> metaQuery,
            in MetaDataSystem system)
        {
            system.SetEntityPaused(entity, paused, metaQuery.GetComponent(entity));
            var childEnumerator = xformQuery.GetComponent(entity).ChildEnumerator;

            while (childEnumerator.MoveNext(out var child))
            {
                RecursiveSetPaused(child.Value, paused, in xformQuery, in metaQuery, in system);
            }
        }

        /// <inheritdoc />
        public void DoMapInitialize(MapId mapId)
        {
            if(!MapExists(mapId))
                throw new ArgumentException("That map does not exist.");

            if (IsMapInitialized(mapId))
                throw new ArgumentException("That map is already initialized.");

            var mapEnt = GetMapEntityId(mapId);
            var mapComp = EntityManager.GetComponent<MapComponent>(mapEnt);
            var xformQuery = EntityManager.GetEntityQuery<TransformComponent>();
            var metaQuery = EntityManager.GetEntityQuery<MetaDataComponent>();
            var metaSystem = EntityManager.EntitySysManager.GetEntitySystem<MetaDataSystem>();

            mapComp.MapPreInit = false;
            mapComp.MapPaused = false;
            EntityManager.Dirty(mapComp);

            RecursiveDoMapInit(mapEnt, in xformQuery, in metaQuery, in metaSystem);
        }

        private void RecursiveDoMapInit(EntityUid entity,
            in EntityQuery<TransformComponent> xformQuery,
            in EntityQuery<MetaDataComponent> metaQuery,
            in MetaDataSystem system)
        {
            // RunMapInit can modify the TransformTree
            // ToArray caches deleted euids, we check here if they still exist.
            if(!metaQuery.TryGetComponent(entity, out var meta))
                return;

            EntityManager.RunMapInit(entity, meta);
            system.SetEntityPaused(entity, false, meta);

            foreach (var child in xformQuery.GetComponent(entity)._children.ToArray())
            {
                RecursiveDoMapInit(child, in xformQuery, in metaQuery, in system);
            }
        }

        /// <inheritdoc />
        public void AddUninitializedMap(MapId mapId)
        {
            SetMapPreInit(mapId);
        }

        private bool CheckMapPause(MapId mapId)
        {
            if(mapId == MapId.Nullspace)
                return false;

            var mapEuid = GetMapEntityId(mapId);

            if (!EntityManager.TryGetComponent<MapComponent>(mapEuid, out var map))
                return false;

            return map.MapPaused;
        }

        private void SetMapPreInit(MapId mapId)
        {
            if(mapId == MapId.Nullspace)
                return;

            var mapEuid = GetMapEntityId(mapId);
            var mapComp = EntityManager.GetComponent<MapComponent>(mapEuid);
            mapComp.MapPreInit = true;
        }

        private bool CheckMapPreInit(MapId mapId)
        {
            if(mapId == MapId.Nullspace)
                return false;

            var mapEuid = GetMapEntityId(mapId);

            if (!EntityManager.TryGetComponent<MapComponent>(mapEuid, out var map))
                return false;

            return map.MapPreInit;
        }

        /// <inheritdoc />
        public bool IsMapPaused(MapId mapId)
        {
            if(mapId == MapId.Nullspace)
                return false;

            var mapEuid = GetMapEntityId(mapId);

            if (!EntityManager.TryGetComponent<MapComponent>(mapEuid, out var map))
                return false;

            return map.MapPaused || map.MapPreInit;
        }

        /// <inheritdoc />
        public bool IsMapInitialized(MapId mapId)
        {
            return !CheckMapPreInit(mapId);
        }

        /// <summary>
        /// Initializes the map pausing system.
        /// </summary>
        private void InitializeMapPausing()
        {
            _conhost.RegisterCommand("pausemap",
                "Pauses a map, pausing all simulation processing on it.",
                "pausemap <map ID>",
                (shell, _, args) =>
                {
                    if (args.Length != 1)
                    {
                        shell.WriteError("Need to supply a valid MapId");
                        return;
                    }

                    var mapId = new MapId(int.Parse(args[0], CultureInfo.InvariantCulture));

                    if (!MapExists(mapId))
                    {
                        shell.WriteError("That map does not exist.");
                        return;
                    }

                    SetMapPaused(mapId, true);
                });

            _conhost.RegisterCommand("querymappaused",
                "Check whether a map is paused or not.",
                "querymappaused <map ID>",
                (shell, _, args) =>
                {
                    var mapId = new MapId(int.Parse(args[0], CultureInfo.InvariantCulture));

                    if (!MapExists(mapId))
                    {
                        shell.WriteError("That map does not exist.");
                        return;
                    }

                    shell.WriteLine(IsMapPaused(mapId).ToString());
                });

            _conhost.RegisterCommand("unpausemap",
                "unpauses a map, resuming all simulation processing on it.",
                "Usage: unpausemap <map ID>",
                (shell, _, args) =>
                {
                    if (args.Length != 1)
                    {
                        shell.WriteLine("Need to supply a valid MapId");
                        return;
                    }

                    var mapId = new MapId(int.Parse(args[0], CultureInfo.InvariantCulture));

                    if (!MapExists(mapId))
                    {
                        shell.WriteLine("That map does not exist.");
                        return;
                    }

                    SetMapPaused(mapId, false);
                });
        }
    }
}
