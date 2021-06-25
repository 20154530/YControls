﻿///------------------------------------------------------------------------------
/// @ Y_Theta
///------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using YControlCore.ControlBase;

namespace YControlCore.ContentControl
{

    /// <summary>
    /// 隐藏菜单控件 用于实现可滑动隐藏的菜单栏
    /// </summary>
    [ContentProperty(nameof(Content))]
    public class Y_CollapseControl : ToggleStateControl
    {
        #region Properties

        private DoubleAnimationUsingKeyFrames _ContentTranslate_Collapsed;
        private DoubleAnimationUsingKeyFrames _ContentTranslate_Expand;
        private DoubleAnimationUsingKeyFrames _Expand_Transition;
        private DoubleAnimationUsingKeyFrames _Collapsed_Transition;
        private DiscreteDoubleKeyFrame _ContentTranslate_Collapsed_F1;
        private SplineDoubleKeyFrame _Collapsed_Transition_F1;

        /// <summary>
        /// 
        /// </summary>
        private static void OnIsExpandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Y_CollapseControl cm = (Y_CollapseControl)d;
            cm.expand((bool)e.NewValue);
        }

        /// <summary>
        /// 
        /// </summary>
        public object Content
        {
            get { return (object)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(object),
                typeof(Y_CollapseControl), new PropertyMetadata(null));

        /// <summary>
        /// 
        /// </summary>
        public ExpandDirection Direction
        {
            get { return (ExpandDirection)GetValue(DirectionProperty); }
            set { SetValue(DirectionProperty, value); }
        }
        public static readonly DependencyProperty DirectionProperty =
            DependencyProperty.Register("Direction", typeof(ExpandDirection),
                typeof(Y_CollapseControl), new PropertyMetadata(ExpandDirection.Bottom, OnOrientationChanged));
        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Y_CollapseControl cm = (Y_CollapseControl)d;
            cm.ChangeOrientation();
        }

        #endregion

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        private void ChangeOrientation()
        {
            if (_ContentTranslate_Collapsed != null)
            {
                ChangeOrientation(Direction.Equals(ExpandDirection.Bottom) || Direction.Equals(ExpandDirection.Top) ?
                                  TranslateTransform.YProperty :
                                  TranslateTransform.XProperty);

            }
            ResetAnimateParam();
        }

        private void ChangeOrientation(object param)
        {
            Storyboard.SetTargetProperty(_ContentTranslate_Expand, new PropertyPath(param));
            Storyboard.SetTargetProperty(_ContentTranslate_Collapsed, new PropertyPath(param));
            Storyboard.SetTargetProperty(_Expand_Transition, new PropertyPath(param));
            Storyboard.SetTargetProperty(_Collapsed_Transition, new PropertyPath(param));
        }

        private void ResetAnimateParam()
        {
            if (_ContentTranslate_Collapsed_F1 != null)
            {
                _Collapsed_Transition_F1.Value = _ContentTranslate_Collapsed_F1.Value
                    = Direction.Equals(ExpandDirection.Bottom) ? -RenderSize.Height :
                      Direction.Equals(ExpandDirection.Right) ? -RenderSize.Width :
                      Direction.Equals(ExpandDirection.Top) ? RenderSize.Height : RenderSize.Width;
            }
        }

        private void expand(bool flag)
        {
            VisualStateManager.GoToState(this, flag ? "Expand" : "Collapsed", UseAnimate);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            ResetAnimateParam();
        }

        public override void OnApplyTemplate()
        {
            _ContentTranslate_Expand = GetTemplateChild("ContentTranslate_Expand") as DoubleAnimationUsingKeyFrames;
            _ContentTranslate_Collapsed = GetTemplateChild("ContentTranslate_Collapsed") as DoubleAnimationUsingKeyFrames;
            _Expand_Transition = GetTemplateChild("Expand_Transition") as DoubleAnimationUsingKeyFrames;
            _Collapsed_Transition = GetTemplateChild("Collapsed_Transition") as DoubleAnimationUsingKeyFrames;
            _ContentTranslate_Collapsed_F1 = GetTemplateChild("ContentTranslate_Collapsed_F1") as DiscreteDoubleKeyFrame;
            _Collapsed_Transition_F1 = GetTemplateChild("Collapsed_Transition_F1") as SplineDoubleKeyFrame;

            base.OnApplyTemplate();
            ChangeOrientation();
        }

        #endregion

        #region Constructors
        static Y_CollapseControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Y_CollapseControl), new FrameworkPropertyMetadata(typeof(Y_CollapseControl)));
            IsExpandProperty.OverrideMetadata(typeof(Y_CollapseControl), new FrameworkPropertyMetadata(true, OnIsExpandChanged));
        }
        #endregion
    }
}
