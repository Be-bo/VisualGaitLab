﻿#pragma checksum "..\..\..\OtherWindows\MeasureWindow.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "EBD5DF7CFD4DF465128EF33E252A5B4446A47F74A41468932ECEA0B039572A29"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using VisualGaitLab.OtherWindows;


namespace VisualGaitLab.OtherWindows {
    
    
    /// <summary>
    /// MeasureWindow
    /// </summary>
    public partial class MeasureWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 24 "..\..\..\OtherWindows\MeasureWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image CanvasBackground;
        
        #line default
        #line hidden
        
        
        #line 25 "..\..\..\OtherWindows\MeasureWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Canvas MeasuringCanvas;
        
        #line default
        #line hidden
        
        
        #line 26 "..\..\..\OtherWindows\MeasureWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Primitives.Thumb Thumbie1;
        
        #line default
        #line hidden
        
        
        #line 27 "..\..\..\OtherWindows\MeasureWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Primitives.Thumb Thumbie2;
        
        #line default
        #line hidden
        
        
        #line 33 "..\..\..\OtherWindows\MeasureWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RadioButton AnalysisTypeRadioTreadmill;
        
        #line default
        #line hidden
        
        
        #line 34 "..\..\..\OtherWindows\MeasureWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RadioButton AnalysisTypeRadioFreeWalking;
        
        #line default
        #line hidden
        
        
        #line 38 "..\..\..\OtherWindows\MeasureWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox DistanceTextBox;
        
        #line default
        #line hidden
        
        
        #line 42 "..\..\..\OtherWindows\MeasureWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TreadmillSpeedTextBox;
        
        #line default
        #line hidden
        
        
        #line 45 "..\..\..\OtherWindows\MeasureWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button CancelButton;
        
        #line default
        #line hidden
        
        
        #line 46 "..\..\..\OtherWindows\MeasureWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ContinueButton;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/VisualGaitLab;component/otherwindows/measurewindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\OtherWindows\MeasureWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.CanvasBackground = ((System.Windows.Controls.Image)(target));
            return;
            case 2:
            this.MeasuringCanvas = ((System.Windows.Controls.Canvas)(target));
            return;
            case 3:
            this.Thumbie1 = ((System.Windows.Controls.Primitives.Thumb)(target));
            
            #line 26 "..\..\..\OtherWindows\MeasureWindow.xaml"
            this.Thumbie1.DragDelta += new System.Windows.Controls.Primitives.DragDeltaEventHandler(this.Thumbie1_DragDelta);
            
            #line default
            #line hidden
            return;
            case 4:
            this.Thumbie2 = ((System.Windows.Controls.Primitives.Thumb)(target));
            
            #line 27 "..\..\..\OtherWindows\MeasureWindow.xaml"
            this.Thumbie2.DragDelta += new System.Windows.Controls.Primitives.DragDeltaEventHandler(this.Thumbie2_DragDelta);
            
            #line default
            #line hidden
            return;
            case 5:
            this.AnalysisTypeRadioTreadmill = ((System.Windows.Controls.RadioButton)(target));
            return;
            case 6:
            this.AnalysisTypeRadioFreeWalking = ((System.Windows.Controls.RadioButton)(target));
            
            #line 34 "..\..\..\OtherWindows\MeasureWindow.xaml"
            this.AnalysisTypeRadioFreeWalking.Checked += new System.Windows.RoutedEventHandler(this.AnalysisTypeRadioFreeWalking_Checked);
            
            #line default
            #line hidden
            
            #line 34 "..\..\..\OtherWindows\MeasureWindow.xaml"
            this.AnalysisTypeRadioFreeWalking.Unchecked += new System.Windows.RoutedEventHandler(this.AnalysisTypeRadioFreeWalking_Unchecked);
            
            #line default
            #line hidden
            return;
            case 7:
            this.DistanceTextBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 38 "..\..\..\OtherWindows\MeasureWindow.xaml"
            this.DistanceTextBox.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.DistanceTextBox_TextChanged);
            
            #line default
            #line hidden
            return;
            case 8:
            this.TreadmillSpeedTextBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 42 "..\..\..\OtherWindows\MeasureWindow.xaml"
            this.TreadmillSpeedTextBox.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.TreadmillSpeedTextBox_TextChanged);
            
            #line default
            #line hidden
            return;
            case 9:
            this.CancelButton = ((System.Windows.Controls.Button)(target));
            
            #line 45 "..\..\..\OtherWindows\MeasureWindow.xaml"
            this.CancelButton.Click += new System.Windows.RoutedEventHandler(this.CancelButton_Click);
            
            #line default
            #line hidden
            return;
            case 10:
            this.ContinueButton = ((System.Windows.Controls.Button)(target));
            
            #line 46 "..\..\..\OtherWindows\MeasureWindow.xaml"
            this.ContinueButton.Click += new System.Windows.RoutedEventHandler(this.ContinueButton_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

