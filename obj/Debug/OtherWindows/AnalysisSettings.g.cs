﻿#pragma checksum "..\..\..\OtherWindows\AnalysisSettings.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "A4DF0A3334DBEBF3427C8240E97436074C505A2686FCEF59CCECBA51EE075825"
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


namespace VisualGaitLab.OtherWindows {
    
    
    /// <summary>
    /// AnalysisSettings
    /// </summary>
    public partial class AnalysisSettings : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 18 "..\..\..\OtherWindows\AnalysisSettings.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image LabelThumbnail;
        
        #line default
        #line hidden
        
        
        #line 19 "..\..\..\OtherWindows\AnalysisSettings.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image LabelPreviewImage;
        
        #line default
        #line hidden
        
        
        #line 22 "..\..\..\OtherWindows\AnalysisSettings.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock CurrentLabelSize;
        
        #line default
        #line hidden
        
        
        #line 23 "..\..\..\OtherWindows\AnalysisSettings.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider LabelSlider;
        
        #line default
        #line hidden
        
        
        #line 25 "..\..\..\OtherWindows\AnalysisSettings.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox SameSizeForAnalysisCheckBox;
        
        #line default
        #line hidden
        
        
        #line 27 "..\..\..\OtherWindows\AnalysisSettings.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button CancelButton;
        
        #line default
        #line hidden
        
        
        #line 28 "..\..\..\OtherWindows\AnalysisSettings.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button SaveSettingsButton;
        
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
            System.Uri resourceLocater = new System.Uri("/VisualGaitLab;component/otherwindows/analysissettings.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\OtherWindows\AnalysisSettings.xaml"
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
            this.LabelThumbnail = ((System.Windows.Controls.Image)(target));
            return;
            case 2:
            this.LabelPreviewImage = ((System.Windows.Controls.Image)(target));
            return;
            case 3:
            this.CurrentLabelSize = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 4:
            this.LabelSlider = ((System.Windows.Controls.Slider)(target));
            
            #line 23 "..\..\..\OtherWindows\AnalysisSettings.xaml"
            this.LabelSlider.ValueChanged += new System.Windows.RoutedPropertyChangedEventHandler<double>(this.LabelSlider_ValueChanged);
            
            #line default
            #line hidden
            return;
            case 5:
            this.SameSizeForAnalysisCheckBox = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 6:
            this.CancelButton = ((System.Windows.Controls.Button)(target));
            
            #line 27 "..\..\..\OtherWindows\AnalysisSettings.xaml"
            this.CancelButton.Click += new System.Windows.RoutedEventHandler(this.CancelButton_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.SaveSettingsButton = ((System.Windows.Controls.Button)(target));
            
            #line 28 "..\..\..\OtherWindows\AnalysisSettings.xaml"
            this.SaveSettingsButton.Click += new System.Windows.RoutedEventHandler(this.SaveSettingsButton_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

