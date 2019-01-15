﻿using Microsoft.Toolkit.Uwp.UI.Animations;
using ReduxSimple.Samples.Extensions;
using System;
using System.Linq;
using System.Reactive.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace ReduxSimple.Samples.TicTacToe
{
    public sealed partial class TicTacToePage : Page
    {
        private static TicTacToeStore _store = new TicTacToeStore();

        public TicTacToePage()
        {
            InitializeComponent();

            // Reset Store (due to HistoryComponent lifecycle)
            _store.Reset();

            // Get UI Elements
            var cellsGrids = CellsRootGrid.Children;

            // Observe changes on state
            _store.ObserveState(state => (state.GameEnded, state.Winner))
                .Subscribe(x =>
                {
                    var (gameEnded, winner) = x;

                    YourTurnTextBlock.HideIf(gameEnded);
                    StartNewGameButton.ShowIf(gameEnded);
                    EndGameTextBlock.ShowIf(gameEnded);

                    if (gameEnded)
                    {
                        if (winner.HasValue)
                            EndGameTextBlock.Text = $"{winner.Value} won!";
                        else
                            EndGameTextBlock.Text = "It's a tie!";
                    }
                });

            _store.ObserveState(state => state.Cells)
                .Subscribe(cells =>
                {
                    for (int i = 0; i < cells.Length; i++)
                    {
                        var cellGrid = cellsGrids[i] as Grid;
                        var textBlock = cellGrid.Children[0] as TextBlock;

                        if (cells[i].Mine.HasValue)
                            textBlock.Text = cells[i].Mine.Value ? "O" : "X";
                        else
                            textBlock.Text = string.Empty;
                    }
                });

            // Observe UI events
            foreach (Grid cellGrid in cellsGrids)
            {
                cellGrid.Events().Tapped
                    .Select(e =>
                    {
                        var grid = e.OriginalSource as Grid;
                        return new { Row = Grid.GetRow(grid), Column = Grid.GetColumn(grid) };
                    })
                    .Where(x =>
                    {
                        var cell = _store.State.Cells.First(c => c.Row == x.Row && c.Column == x.Column);
                        return !_store.State.GameEnded && !cell.Mine.HasValue;
                    })
                    .Subscribe(x =>
                    {
                        _store.Dispatch(new PlayAction { Row = x.Row, Column = x.Column });
                    });
            }

            StartNewGameButton.Events().Click
                .Subscribe(_ => _store.Dispatch(new StartNewGameAction()));

            // Initialize Components
            HistoryComponent.Initialize(_store);

            // Initialize Documentation
            DocumentationComponent.LoadMarkdownFilesAsync("TicTacToe");

            ContentGrid.Events().Tapped
                .Subscribe(_ => DocumentationComponent.Collapse());
            DocumentationComponent.ObserveOnExpanded()
                .Subscribe(_ => ContentGrid.Blur(5).Start());
            DocumentationComponent.ObserveOnCollapsed()
                .Subscribe(_ => ContentGrid.Blur(0).Start());
        }
    }
}