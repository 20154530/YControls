﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace YControls.SlideControls {
    /// <summary>
    /// 自定义ScrollViewer
    /// 暴露scrollbar，并添加一些动画效果
    /// </summary>
    public sealed class YT_ScrollViewer : ScrollViewer {
        #region Properties

        #region Animation

        #region Aim

        private Grid _vScrollPanel;

        private Grid _hScrollPanel;

        private ScrollBar _vScrollBar;

        private ScrollBar _hScrollBar;

        private TranslateTransform _verticalTranslate;

        private TranslateTransform _horizontalTranslate;
        #endregion

        #region Transform

        private DoubleAnimationUsingKeyFrames _fadeinanimation;

        private SplineDoubleKeyFrame _keyin1;

        private DoubleAnimationUsingKeyFrames _fadeoutanimation;

        private SplineDoubleKeyFrame _keyout1;

        private DoubleAnimationUsingKeyFrames _hideanimation;

        private SplineDoubleKeyFrame _keyhide1;

        private DoubleAnimationUsingKeyFrames _showanimation;

        private SplineDoubleKeyFrame _keyshow1;

        #endregion
        #endregion
        /// <summary>
        /// 滚动条布局
        /// </summary>
        public ScrollbarLayout ScrollbarLayout {
            get { return (ScrollbarLayout)GetValue(ScrollbarLayoutProperty); }
            set { SetValue(ScrollbarLayoutProperty, value); }
        }
        public static readonly DependencyProperty ScrollbarLayoutProperty =
            DependencyProperty.Register("ScrollbarLayout", typeof(ScrollbarLayout),
                typeof(YT_ScrollViewer), new PropertyMetadata(ScrollbarLayout.Over, OnScrollbarLayoutChanged));
        private static void OnScrollbarLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if((ScrollbarLayout)e.NewValue == ScrollbarLayout.Dock) {
                ((YT_ScrollViewer)d).DisposeMiniable();
                ((YT_ScrollViewer)d).DisposeAutoHide();
            }
            else {
                if (((YT_ScrollViewer)d).AutoHideScrollbar)
                    ((YT_ScrollViewer)d).InitAutoHide();
                if(((YT_ScrollViewer)d).Miniable)
                    ((YT_ScrollViewer)d).InitMiniable();
            }
        }

        /// <summary>
        /// 是否自动隐藏滚动条
        /// 仅在滚动条布局为Over时有效
        /// </summary>
        public bool AutoHideScrollbar {
            get { return (bool)GetValue(AutoHideScrollbarProperty); }
            set { SetValue(AutoHideScrollbarProperty, value); }
        }
        public static readonly DependencyProperty AutoHideScrollbarProperty =
            DependencyProperty.Register("AutoHideScrollbar", typeof(bool),
                typeof(YT_ScrollViewer), new PropertyMetadata(true, OnAutoHideScrollbarChanged));
        private static void OnAutoHideScrollbarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if ((bool)e.NewValue)
                ((YT_ScrollViewer)d).InitAutoHide();
            else
                ((YT_ScrollViewer)d).DisposeAutoHide();
        }

        /// <summary>
        /// 是否自动收缩
        /// 仅在滚动条布局为Over时有效
        /// </summary>
        public bool Miniable {
            get { return (bool)GetValue(MiniableProperty); }
            set { SetValue(MiniableProperty, value); }
        }
        public static readonly DependencyProperty MiniableProperty =
            DependencyProperty.Register("Miniable", typeof(bool),
                typeof(YT_ScrollViewer), new PropertyMetadata(true, OnMiniableChanged));
        private static void OnMiniableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if ((bool)e.NewValue)
                ((YT_ScrollViewer)d).InitMiniable();
            else
                ((YT_ScrollViewer)d).DisposeMiniable();
        }

        /// <summary>
        /// 滚动条的宽度
        /// </summary>
        public double ScrollbarThickness {
            get { return (double)GetValue(ScrollbarThicknessProperty); }
            set { SetValue(ScrollbarThicknessProperty, value); }
        }
        public static readonly DependencyProperty ScrollbarThicknessProperty =
            DependencyProperty.Register("ScrollbarThickness", typeof(double),
                typeof(YT_ScrollViewer), new PropertyMetadata(24.0, OnScrollbarThicknessChanged));
        private static void OnScrollbarThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (((YT_ScrollViewer)d)._keyin1 != null)
                ((YT_ScrollViewer)d)._keyin1.Value = (double)e.NewValue - ((YT_ScrollViewer)d).ScrollbarMiniThickness;
        }

        /// <summary>
        /// 滚动条收缩起来的宽度
        /// </summary>
        public double ScrollbarMiniThickness {
            get { return (double)GetValue(ScrollbarMiniThicknessProperty); }
            set { SetValue(ScrollbarMiniThicknessProperty, value); }
        }
        public static readonly DependencyProperty ScrollbarMiniThicknessProperty =
            DependencyProperty.Register("ScrollbarMiniThickness", typeof(double),
                typeof(YT_ScrollViewer), new PropertyMetadata(2.0, OnScrollbarMiniThicknessChanged));
        private static void OnScrollbarMiniThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (((YT_ScrollViewer)d)._keyin1 != null)
                ((YT_ScrollViewer)d)._keyin1.Value = ((YT_ScrollViewer)d).ScrollbarThickness - (double)e.NewValue;
        }
        #endregion

        #region Methods

        public void ScrollBarMini() {
            _verticalTranslate.BeginAnimation(TranslateTransform.XProperty, _fadeinanimation);
            _horizontalTranslate.BeginAnimation(TranslateTransform.YProperty, _fadeinanimation);
        }

        public void ScrollBarNormal() {
            _verticalTranslate.BeginAnimation(TranslateTransform.XProperty, _fadeoutanimation);
            _horizontalTranslate.BeginAnimation(TranslateTransform.YProperty, _fadeoutanimation);
        }

        public void HideScrollbar() {
            _vScrollBar.BeginAnimation(ScrollBar.OpacityProperty, _hideanimation);
            _hScrollBar.BeginAnimation(ScrollBar.OpacityProperty, _hideanimation);
        }

        public void ShowScrollbar() {
            _vScrollBar.BeginAnimation(ScrollBar.OpacityProperty, _showanimation);
            _hScrollBar.BeginAnimation(ScrollBar.OpacityProperty, _showanimation);
        }

        private void InitAutoHide() {
            _vScrollBar = GetTemplateChild("PART_VerticalScrollBar") as ScrollBar;
            _hScrollBar = GetTemplateChild("PART_HorizontalScrollBar") as ScrollBar;
            MouseEnter += YT_ScrollViewer_MouseEnter;
            MouseLeave += YT_ScrollViewer_MouseLeave;

            _hideanimation = new DoubleAnimationUsingKeyFrames();
            _keyhide1 = new SplineDoubleKeyFrame {
                KeySpline = new KeySpline(0, 0.4, 0.6, 1),
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(320)),
                Value = 0,
            };
            _hideanimation.KeyFrames.Add(_keyhide1);

            _showanimation = new DoubleAnimationUsingKeyFrames();
            _keyshow1 = new SplineDoubleKeyFrame {
                KeySpline = new KeySpline(0, 0.4, 0.6, 1),
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(320)),
                Value = 1,
            };
            _showanimation.KeyFrames.Add(_keyshow1);

            HideScrollbar();
        }

        private void DisposeAutoHide() {
            if (_vScrollBar != null)
                ShowScrollbar();

            MouseEnter -= YT_ScrollViewer_MouseEnter;
            MouseLeave -= YT_ScrollViewer_MouseLeave;
            _vScrollBar = null;
            _hScrollBar = null;
        }

        private void InitMiniable() {
            _vScrollPanel = GetTemplateChild("VerticalScrollBarPanel") as Grid;
            _vScrollPanel.MouseEnter += _ScrollPanel_MouseEnter;
            _vScrollPanel.MouseLeave += _ScrollPanel_MouseLeave;
            _hScrollPanel = GetTemplateChild("HorizontalScrollBarPanel") as Grid;
            _hScrollPanel.MouseEnter += _ScrollPanel_MouseEnter;
            _hScrollPanel.MouseLeave += _ScrollPanel_MouseLeave;
            _verticalTranslate = GetTemplateChild("VerticalTranslate") as TranslateTransform;
            _horizontalTranslate = GetTemplateChild("HorizontalTranslate") as TranslateTransform;

            _fadeinanimation = new DoubleAnimationUsingKeyFrames {
                BeginTime = TimeSpan.FromMilliseconds(500),
            };
            _keyin1 = new SplineDoubleKeyFrame {
                KeySpline = new KeySpline(0, 0.4, 0.6, 1),
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(320)),
                Value = ScrollbarThickness - ScrollbarMiniThickness,
            };
            _fadeinanimation.KeyFrames.Add(_keyin1);

            _fadeoutanimation = new DoubleAnimationUsingKeyFrames();
            _keyout1 = new SplineDoubleKeyFrame {
                KeySpline = new KeySpline(0, 0.4, 0.6, 1),
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(320)),
                Value = 0,
            };
            _fadeoutanimation.KeyFrames.Add(_keyout1);

            ScrollBarMini();
        }

        private void DisposeMiniable() {
            if (_vScrollPanel != null)
                ScrollBarNormal();

            _vScrollPanel.MouseEnter -= _ScrollPanel_MouseEnter;
            _vScrollPanel.MouseLeave -= _ScrollPanel_MouseLeave;
            _hScrollPanel.MouseEnter -= _ScrollPanel_MouseEnter;
            _hScrollPanel.MouseLeave -= _ScrollPanel_MouseLeave;
            _vScrollPanel = null;
            _hScrollPanel = null;
            _verticalTranslate = null;
            _horizontalTranslate = null;
        }

        private void YT_ScrollViewer_MouseLeave(object sender, MouseEventArgs e) {
            HideScrollbar();
        }

        private void YT_ScrollViewer_MouseEnter(object sender, MouseEventArgs e) {
            ShowScrollbar();
        }

        private void _ScrollPanel_MouseLeave(object sender, MouseEventArgs e) {
            ScrollBarMini();
        }

        private void _ScrollPanel_MouseEnter(object sender, MouseEventArgs e) {
            ScrollBarNormal();
        }

        #endregion

        #region Override
        public override void OnApplyTemplate() {
            if (ScrollbarLayout == ScrollbarLayout.Over) {
                if (AutoHideScrollbar)
                    InitAutoHide();
                if (Miniable)
                    InitMiniable();
            }
            base.OnApplyTemplate();
        }
        #endregion

        #region Constructors
        public YT_ScrollViewer() : base() {

        }

        static YT_ScrollViewer() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(YT_ScrollViewer), new FrameworkPropertyMetadata(typeof(YT_ScrollViewer)));
        }
        #endregion
    }

}