using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Robust.Shared.GameObjects;

namespace Robust.Server.Bql
{
    [Flags]
    [PublicAPI]
    public enum QuerySelectorArgument
    {
        Integer    = 0b00000001,
        Float      = 0b00000010,
        String     = 0b00000100,
        Percentage = 0b00001000,
        Component  = 0b00010000,
        //SubQuery   = 0b00100000,
        EntityId   = 0b01000000,
    }

    [PublicAPI]
    public abstract class BqlQuerySelector
    {
        /// <summary>
        /// The token name for the given QuerySelector, for example `when`.
        /// </summary>
        public virtual string Token => throw new NotImplementedException();

        /// <summary>
        /// Arguments for the given QuerySelector, presented as "what arguments are permitted in what spot".
        /// </summary>
        public virtual QuerySelectorArgument[] Arguments => throw new NotImplementedException();

        /// <summary>
        /// Performs a transform over it's input entity list, whether that be filtering (selecting) or expanding the
        /// input on some criteria like what entities are nearby.
        /// </summary>
        /// <param name="input">Input entity list.</param>
        /// <param name="arguments">Parsed selector arguments.</param>
        /// <param name="isInverted">Whether the query is inverted.</param>
        /// <param name="entityManager">The entity manager.</param>
        /// <returns>New list of entities</returns>
        /// <exception cref="NotImplementedException">someone is a moron if this happens.</exception>
        public abstract IEnumerable<EntityUid> DoSelection(IEnumerable<EntityUid> input,
            IReadOnlyList<object> arguments, bool isInverted, IEntityManager entityManager);

        /// <summary>
        /// Performs selection as the first selector in the query. Allows for optimizing when you can be more efficient
        /// than just querying every entity.
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="isInverted"></param>
        /// <param name="entityManager"></param>
        /// <returns></returns>
        public virtual IEnumerable<EntityUid> DoInitialSelection(IReadOnlyList<object> arguments, bool isInverted, IEntityManager entityManager)
        {
            return DoSelection(entityManager.GetEntities(), arguments, isInverted, entityManager);
        }

        [UsedImplicitly]
        protected BqlQuerySelector() {}
    }
}
