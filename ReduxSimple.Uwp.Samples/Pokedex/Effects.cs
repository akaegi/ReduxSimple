﻿using ReduxSimple.Uwp.Samples.Extensions;
using System;
using System.Linq;
using System.Reactive.Linq;
using static ReduxSimple.Effects;
using static ReduxSimple.Uwp.Samples.App;
using static ReduxSimple.Uwp.Samples.Pokedex.Selectors;

namespace ReduxSimple.Uwp.Samples.Pokedex
{
    public static class Effects
    {
        private readonly static PokedexApiClient _pokedexApiClient = new PokedexApiClient();

        public static Effect<RootState> LoadPokemonList = CreateEffect<RootState>(
            () => Store.ObserveAction<GetPokemonListAction>()
                .Select(_ => 
                    _pokedexApiClient.GetPokedex()
                        .Select(response =>
                        {
                            return new GetPokemonListFullfilledAction
                            {
                                Pokedex = response.PokemonEntries
                                    .Select(p => new PokemonGeneralInfo { Id = p.Number, Name = p.Species.Name.Capitalize() })
                                    .ToList()
                            };
                        })
                        .Catch<object, Exception>(e =>
                        {
                            return Observable.Return(
                                new GetPokemonListFailedAction
                                {
                                    Exception = e
                                }
                            );
                        })
                )
                .Switch(),
            true
        );

        public static Effect<RootState> LoadPokemonById = CreateEffect<RootState>(
            () => Store.ObserveAction<GetPokemonByIdAction>()
                .Select(action => 
                    _pokedexApiClient.GetPokemonById(action.Id)
                        .Select(response =>
                        {
                            return new GetPokemonByIdFullfilledAction
                            {
                                Pokemon = new Pokemon
                                {
                                    Id = response.Id,
                                    Name = response.Name.Capitalize(),
                                    Image = response.Sprites.Image,
                                    Types = response.Types
                                        .OrderBy(t => t.Slot)
                                        .Select(t => new PokemonType { Name = t.Type.Name })
                                        .ToList()
                                }
                            };
                        })
                        .Catch<object, Exception>(e =>
                        {
                            return Observable.Return(
                                new GetPokemonByIdFailedAction
                                {
                                    Exception = e
                                }
                            );
                        })
                )
                .Switch(),                
            true
        );

        public static Effect<RootState> SearchPokemon = CreateEffect<RootState>(
            () => Store.Select(SelectSearch)
                .Select(search =>
                {
                    return Observable.CombineLatest(
                        Observable.Return(search),
                        Store.Select(SelectIsPokedexEmpty).Take(1),
                        Store.Select(SelectPokedex).Take(1),
                        Store.Select(SelectSuggestions, 1).Take(1),
                        Tuple.Create
                    );
                })
                .Switch()
                .Where(x =>
                {
                    var (search, isPokedexEmpty, _, _) = x;
                    return !isPokedexEmpty;
                })
                .Select(x =>
                {
                    var (search, _, pokedex, suggestions) = x;

                    if (!string.IsNullOrWhiteSpace(search))
                    {
                        // Search Pokemon by Id
                        if (
                            search.StartsWith("#") &&
                            int.TryParse(search.Substring(1), out int searchedId)
                        )
                        {
                            if (pokedex.Any(p => p.Id == searchedId))
                                return new GetPokemonByIdAction { Id = searchedId };

                            return new ResetPokemonAction();
                        }

                        // Search Pokemon by both Id and Name
                        if (suggestions.Any())
                            return new GetPokemonByIdAction { Id = suggestions.First().Id };
                    }

                    return new ResetPokemonAction() as object;
                }),
            true
        );
    }
}
