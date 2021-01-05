﻿using Coffee.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Coffee
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            BindingContext = viewModels = new OrderViewModel();
        }

        AnimationStateMachine animStates = new AnimationStateMachine();

        enum PageStates
        {
            Closed,
            Peek,
            Open
        }

        private double pageHeight;
        private double thumbHeight = 25;
        private double openThreshold = -300; // should be 1/3 of the screen

        protected override void OnAppearing()
        {
            base.OnAppearing();
            CentsFlip.AnimationOffset = new Point(0, 20);
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            pageHeight = height;
            SetupStates();
        }


        private void SetupStates()
        {
            animStates = new AnimationStateMachine();

            animStates.Add(PageStates.Closed, new ViewTransition[] {
                new ViewTransition(FrontCard, AnimationType.TranslationY, 0),
                new ViewTransition(Thumb,  AnimationType.Opacity, 0),
            });

            animStates.Add(PageStates.Peek, new ViewTransition[] {
                new ViewTransition(FrontCard, AnimationType.TranslationY, -100),
                new ViewTransition(Thumb,  AnimationType.Opacity, 1),
            });

            animStates.Add(PageStates.Open, new ViewTransition[] {
                new ViewTransition(FrontCard, AnimationType.TranslationY, -(this.Height - 25)),
                new ViewTransition(Thumb,  AnimationType.Opacity, 1),
            });
        }

        double lastVelocity;
        private OrderViewModel viewModels;

        private void PanGestureRecognizer_PanUpdated(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Running:
                    lastVelocity = e.TotalY;
                    FrontCard.TranslationY += e.TotalY;
                    break;
                case GestureStatus.Completed:
                    if (lastVelocity < -10)
                        animStates.Go(PageStates.Open);
                    else if (lastVelocity > 10)
                        animStates.Go(PageStates.Peek);
                    else
                    {
                        if (FrontCard.TranslationY < openThreshold)
                            animStates.Go(PageStates.Open);
                        else
                            animStates.Go(PageStates.Peek);
                    }
                    break;
            }
        }

        private void AddToBag_Clicked(object sender, EventArgs e)
        {
            animStates.Go(PageStates.Peek);
        }

        private void SwipeUp_Swiped(object sender, SwipedEventArgs e)
        {
            animStates.Go(PageStates.Open);
        }

        private void SwipeDown_Swiped(object sender, SwipedEventArgs e)
        {
            animStates.Go(PageStates.Peek);
        }

        private enum SelectedSize
        {
            small,
            medium,
            large
        }

        private void UpdateSizeSelection(SelectedSize selectedSize)
        {
            Xamarin.Forms.Shapes.Rectangle selectedRect = SmallRect;
            Xamarin.Forms.Shapes.Path selectedCup = SmallCup;
            switch (selectedSize)
            {
                case SelectedSize.small:
                    selectedRect = SmallRect;
                    selectedCup = SmallCup;
                    break;
                case SelectedSize.medium:
                    selectedRect = MediumRect;
                    selectedCup = MediumCup;
                    break;
                case SelectedSize.large:
                    selectedRect = LargeRect;
                    selectedCup = LargeCup;
                    break;
            }

            // clear the selections
            SolidColorBrush unselectedBackground = new SolidColorBrush(Color.FromHex("F8F8F8"));
            SolidColorBrush unselectedStroke = new SolidColorBrush(Color.FromHex("E1E1E1"));
            SolidColorBrush selectedBackground = new SolidColorBrush(Color.FromHex("E5F0EC"));
            SolidColorBrush selectedStroke = new SolidColorBrush(Color.FromHex("1B714B"));

            SmallRect.Fill = unselectedBackground;
            SmallRect.Stroke = unselectedStroke;
            MediumRect.Fill = unselectedBackground;
            MediumRect.Stroke = unselectedStroke;
            LargeRect.Fill = unselectedBackground;
            LargeRect.Stroke = unselectedStroke;
            SmallCup.Stroke = unselectedStroke;
            MediumCup.Stroke = unselectedStroke;
            LargeCup.Stroke = unselectedStroke;

            // move the selectionRectangle
            SelectionRect.LayoutTo(selectedRect.Bounds, 100, Easing.CubicInOut);

            // update the background colors
            selectedRect.Fill = selectedBackground;

            // update the stroke on the cup
            selectedCup.Stroke = selectedStroke;
        }

        private void SmallSized_Tapped(object sender, EventArgs e)
        {
            UpdateSizePrice(viewModels.SelectedItem.SmallPrice);
            UpdateSizeSelection(SelectedSize.small);
        }

        private void MediumSized_Tapped(object sender, EventArgs e)
        {
            UpdateSizePrice(viewModels.SelectedItem.MediumPrice);
            UpdateSizeSelection(SelectedSize.medium);
        }

        private void LargeSized_Tapped(object sender, EventArgs e)
        {
            UpdateSizePrice(viewModels.SelectedItem.LargePrice);
            UpdateSizeSelection(SelectedSize.large);
        }


        private void UpdateSizePrice(decimal price)
        {
            var textPrice = price.ToString("0.00");
            var decimalLocation = textPrice.IndexOf('.');
            // split out the dollar and cents
            string dollar = textPrice.Substring(0, decimalLocation);
            string cents = textPrice.Substring(decimalLocation+1, textPrice.Length - (decimalLocation+1));

            CentsFlip.Text = cents;
            DollarFlip.Text = dollar;
        }

    }




}
