using System;
using System.Collections.Generic;
using System.Linq;
using Silphid.Extensions;
using UnityEngine;
using Zenject;

namespace Silphid.Showzup
{
    public class ViewResolver : IViewResolver, IInitializable
    {
        private struct MappingCandidate
        {
            public ViewMapping Mapping { get; }
            private int VariantScore { get; }
            private int TypeScore { get; }

            public MappingCandidate(ViewMapping mapping, int variantScore, int typeScore)
            {
                Mapping = mapping;
                VariantScore = variantScore;
                TypeScore = typeScore;
            }

            public override string ToString() =>
                $"{Mapping} (VariantScore: {VariantScore}, TypeScore: {TypeScore})";
        }

        private const string DefaultCategory = "Default";
        private const int ZeroScore = 0;
        private const int MediumScore = 50;
        private const int HighScore = 100;

        private readonly List<ViewMapping> _mappings = new List<ViewMapping>();
        private readonly IGlobalVariantProvider _globalVariantProvider;

        public ViewResolver([InjectOptional] IGlobalVariantProvider globalVariantProvider = null)
        {
            _globalVariantProvider = globalVariantProvider;
        }

        public void Initialize()
        {
            _mappings.AddRange(
                from viewType in GetAllViewTypes()
                let viewModelType = GetViewModelType(viewType)
                let viewVariants = viewType.GetAttributes<VariantAttribute>().Select(x => x.Variant).ToArray()
                from assetAttribute in viewType.GetRequiredAttributes<AssetAttribute>()
                select new ViewMapping(viewModelType, viewType, assetAttribute.Uri, viewVariants.Concat(assetAttribute.Variants)));
        }

        private Type GetViewModelType(Type viewType)
        {
            var viewModel = viewType
                .SelfAndAncestors()
                .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(View<>))
                ?.GetGenericArguments()
                .FirstOrDefault();

            if (viewModel == null)
                throw new InvalidOperationException($"Could not determine view model associated with view type {viewType.Name}");

            return viewModel;
        }

        private IEnumerable<Type> GetAllViewTypes() =>
            from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
            from candidateType in domainAssembly.GetTypes()
            where typeof(IView).IsAssignableFrom(candidateType) && !candidateType.IsAbstract
            select candidateType;

        public ViewMapping ResolveFromViewModelType(Type viewModelType, Options options = null) =>
            ResolveInternal(viewModelType, mapping => GetTypeScore(viewModelType, mapping.ViewModelType), options);

        public ViewMapping ResolveFromViewType(Type viewType, Options options = null) =>
            ResolveInternal(viewType, mapping => GetTypeScore(viewType, mapping.ViewType), options);

        private ViewMapping ResolveInternal(Type type, Func<ViewMapping, int> getTypeSpecificity, Options options)
        {
            var globalVariants = _globalVariantProvider?.Variants ?? Enumerable.Empty<string>();
            var variants = options.GetVariants().Concat(globalVariants).ToList();

            //Debug.Log($"#Views# Resolving view for {type} and variants {variants.ToDelimitedString(";")}");

            var candidates = (
                    from mapping in _mappings
                    let variantScore = GetVariantScore(variants, mapping.Variants.ToList())
                    let viewModelScore = getTypeSpecificity(mapping)
                    where viewModelScore != ZeroScore
                    orderby variantScore descending, viewModelScore descending
                    select new MappingCandidate(mapping, variantScore, viewModelScore))
				.ToList();

            var resolved = candidates.FirstOrDefault();
            //Debug.Log($"#Views# Resolved: {resolved})");

            //if (candidates.Count > 1)
            //    Debug.Log($"#Views# Other candidates:{Environment.NewLine}" +
            //              $"{candidates.Skip(1).ToDelimitedString(Environment.NewLine)}");

            return resolved.Mapping;
        }

        private int GetVariantScore(IEnumerable<string> requestedVariants, IList<string> candidateVariants) =>
			requestedVariants.Count(candidateVariants.Contains) * HighScore +
			(candidateVariants.Contains(DefaultCategory) ? MediumScore : ZeroScore);

        private int GetTypeScore(Type requestedType, Type candidateType)
        {
            var score = HighScore;
            var type = candidateType;
            while (type != null)
            {
                if (type == requestedType)
                    return score;

                score--;
                type = type.BaseType;
            }

            return ZeroScore;
        }
    }
}