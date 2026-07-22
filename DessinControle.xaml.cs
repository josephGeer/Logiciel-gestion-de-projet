using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using app_test.Items;

namespace app_test
{
    public sealed partial class DessinControle : UserControl
    {
        public DessinControle()
        {
            InitializeComponent();
        }

        private Brush currentBrush = new SolidColorBrush(Microsoft.UI.Colors.Black);
        private bool IsDrawing = false;
        private Point startPoint;
        private int SelectIndex = 0;

        private Dessin.TraitDessin CurrentTrait;
        private Dessin CurrentDessin;

        private enum OutilDessin { Crayon, Pinceau, Gomme, Pipette }
        private OutilDessin outilActuel = OutilDessin.Crayon;

        public List<double> BrushThickness { get; } = new List<double>()
        {
            4,
            8,
            18,
            20,
            31,
            42,
            54,
            66
        };

        private void DessinControle_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (ReferenceEquals(args.NewValue, CurrentDessin)) return;

            canvas.Children.Clear();
            CurrentDessin = args.NewValue as Dessin;
            CurrentTrait = null;
            IsDrawing = false;

            ChargerTraits();
        }

        private void ChargerTraits()
        {
            if (CurrentDessin?.Traits == null) return;

            foreach (var trait in CurrentDessin.Traits)
            {
                Polyline polyline = new Polyline
                {
                    Stroke = ConvertirCouleur(trait.Color),
                    StrokeThickness = trait.Thickness,
                    StrokeLineJoin = PenLineJoin.Round,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round
                };

                PointCollection points = new PointCollection();
                foreach (var p in trait.ListePoint)
                    points.Add(p);

                polyline.Points = points;
                canvas.Children.Add(polyline);
            }
        }

        private Brush ConvertirCouleur(string hex)
        {
            var color = (Windows.UI.Color)Microsoft.UI.Xaml.Markup.XamlBindingHelper
                .ConvertValue(typeof(Windows.UI.Color), hex);
            return new SolidColorBrush(color);
        }

        private async void Canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
            if (!e.Pointer.PointerDeviceType.Equals(PointerDeviceType.Mouse)) return;

            if (outilActuel == OutilDessin.Pipette)
            {
                var pointerPosition = e.GetCurrentPoint(canvas);
                int x = (int)pointerPosition.Position.X;
                int y = (int)pointerPosition.Position.Y;

                RenderTargetBitmap renderBitmap = new RenderTargetBitmap();
                await renderBitmap.RenderAsync(canvas);

                var pixelBuffer = await renderBitmap.GetPixelsAsync();
                var pixelData = pixelBuffer.ToArray();

                int pixelIndex = (y * renderBitmap.PixelWidth + x) * 4;

                byte[] pixelColor = new byte[4];
                Array.Copy(pixelData, pixelIndex, pixelColor, 0, 4);

                Windows.UI.Color color = Windows.UI.Color.FromArgb(pixelColor[3], pixelColor[2], pixelColor[1], pixelColor[0]);

                CurrentColor.Background = currentBrush = new SolidColorBrush(color);
                return;
            }

            IsDrawing = true;
            startPoint = e.GetCurrentPoint(canvas).Position;

            Windows.UI.Color traitColor = ((SolidColorBrush)currentBrush).Color;
            CurrentTrait = new Dessin.TraitDessin(
                new List<Point> { new Point(startPoint.X, startPoint.Y) },
                traitColor,
                BrushThickness[SelectIndex],
                outilActuel == OutilDessin.Gomme
            );
        }

        private void Canvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!IsDrawing) return;
            var currentPosition = e.GetCurrentPoint(canvas).Position;

            if (outilActuel == OutilDessin.Pinceau)
            {
                Line line = new Line
                {
                    X1 = startPoint.X,
                    Y1 = startPoint.Y,
                    X2 = currentPosition.X,
                    Y2 = currentPosition.Y,
                    Stroke = currentBrush,
                    StrokeThickness = BrushThickness[SelectIndex],
                    StrokeEndLineCap = PenLineCap.Round,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeLineJoin = PenLineJoin.Round
                };

                Line lineBlur = new Line
                {
                    X1 = startPoint.X,
                    Y1 = startPoint.Y,
                    X2 = currentPosition.X,
                    Y2 = currentPosition.Y,
                    Stroke = currentBrush,
                    StrokeThickness = BrushThickness[SelectIndex] + 10,
                    StrokeEndLineCap = PenLineCap.Round,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeLineJoin = PenLineJoin.Round,
                    Opacity = 0.2,
                };

                canvas.Children.Add(lineBlur);
                canvas.Children.Add(line);
            }
            else
            {
                Line line = new Line
                {
                    X1 = startPoint.X,
                    Y1 = startPoint.Y,
                    X2 = currentPosition.X,
                    Y2 = currentPosition.Y,
                    Stroke = currentBrush,
                    StrokeThickness = BrushThickness[SelectIndex],
                    StrokeEndLineCap = PenLineCap.Round,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeLineJoin = PenLineJoin.Round
                };

                canvas.Children.Add(line);
            }

            startPoint = currentPosition;
            CurrentTrait?.ListePoint.Add(currentPosition);
        }

        private void Canvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (CurrentTrait != null)
                CurrentDessin?.Traits.Add(CurrentTrait);

            CurrentTrait = null;
            IsDrawing = false;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            canvas.Children.Clear();
            CurrentDessin?.Traits.Clear();
        }

        private void ColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            currentBrush = new SolidColorBrush(args.NewColor);
            CurrentColor.Background = new SolidColorBrush(args.NewColor);
        }

        private void comboBoxBrushThickness_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectIndex = ComboBoxBrushThickness.SelectedIndex;
        }

        private void SelectionnerOutil(OutilDessin outil)
        {
            outilActuel = outil;

            pencilButton.IsChecked = (outil == OutilDessin.Crayon);
            brushButton.IsChecked = (outil == OutilDessin.Pinceau);
            eraserButton.IsChecked = (outil == OutilDessin.Gomme);
            colorPickerButton.IsChecked = (outil == OutilDessin.Pipette);

            if (outil == OutilDessin.Gomme)
                currentBrush = new SolidColorBrush(Microsoft.UI.Colors.White);
        }

        private void PencilButton_Click(object sender, RoutedEventArgs e) => SelectionnerOutil(OutilDessin.Crayon);
        private void BrushButton_Click(object sender, RoutedEventArgs e) => SelectionnerOutil(OutilDessin.Pinceau);
        private void EraserButton_Click(object sender, RoutedEventArgs e) => SelectionnerOutil(OutilDessin.Gomme);
        private void PickerButton_Click(object sender, RoutedEventArgs e) => SelectionnerOutil(OutilDessin.Pipette);

        private void Sizer_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
        }

        
    }
}