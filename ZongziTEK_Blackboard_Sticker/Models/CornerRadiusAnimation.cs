using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace ZongziTEK_Blackboard_Sticker.Models
{
    public class CornerRadiusAnimation : AnimationTimeline
    {
        public CornerRadius From
        {
            get => (CornerRadius)GetValue(FromProperty);
            set => SetValue(FromProperty, value);
        }

        public static readonly DependencyProperty FromProperty =
            DependencyProperty.Register("From", typeof(CornerRadius), typeof(CornerRadiusAnimation));

        public CornerRadius To
        {
            get => (CornerRadius)GetValue(ToProperty);
            set => SetValue(ToProperty, value);
        }

        public static readonly DependencyProperty ToProperty =
            DependencyProperty.Register("To", typeof(CornerRadius), typeof(CornerRadiusAnimation));

        public EasingFunctionBase EasingFunction
        {
            get => (EasingFunctionBase)GetValue(EasingFunctionProperty);
            set => SetValue(EasingFunctionProperty, value);
        }

        public static readonly DependencyProperty EasingFunctionProperty =
            DependencyProperty.Register("EasingFunction", typeof(EasingFunctionBase), typeof(CornerRadiusAnimation));

        public override Type TargetPropertyType => typeof(CornerRadius);

        protected override Freezable CreateInstanceCore()
        {
            return new CornerRadiusAnimation();
        }

        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            if (animationClock.CurrentProgress == null)
                return From;

            double progress = animationClock.CurrentProgress.Value;

            // 如果设置了 EasingFunction，则使用它来调整进度
            if (EasingFunction != null)
            {
                progress = EasingFunction.Ease(progress);
            }

            var fromRadius = From;
            var toRadius = To;

            return new CornerRadius(
                fromRadius.TopLeft + (toRadius.TopLeft - fromRadius.TopLeft) * progress,
                fromRadius.TopRight + (toRadius.TopRight - fromRadius.TopRight) * progress,
                fromRadius.BottomRight + (toRadius.BottomRight - fromRadius.BottomRight) * progress,
                fromRadius.BottomLeft + (toRadius.BottomLeft - fromRadius.BottomLeft) * progress
            );
        }
    }
}
