﻿#pragma checksum "..\..\..\OtherWindows\BitrateWindow.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "347AE9BD94EBEC5601E265618B335C831FE258340F66B3879A27B7A3556D48E3"
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
    /// BitrateWindow
    /// </summary>
    public partial class BitrateWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 17 "..\..\..\OtherWindows\BitrateWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock BitrateNumberText;
        
        #line default
        #line hidden
        
        
        #line 18 "..\..\..\OtherWindows\BitrateWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider BitrateSlider;
        
        #line default
        #line hidden
        
        
        #line 22 "..\..\..\OtherWindows\BitrateWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BitrateSaveButton;
        
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
            System.Uri resourceLocater = new System.Uri("/VisualGaitLab;component/otherwindows/bitratewindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\OtherWindows\BitrateWindow.xaml"
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
            this.BitrateNumberText = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 2:
            this.BitrateSlider = ((System.Windows.Controls.Slider)(target));
            
            #line 18 "..\..\..\OtherWindows\BitrateWindow.xaml"
            this.BitrateSlider.ValueChanged += new System.Windows.RoutedPropertyChangedEventHandler<double>(this.BitrateSlider_ValueChanged);
            
            #line default
            #line hidden
            return;
            case 3:
            this.BitrateSaveButton = ((System.Windows.Controls.Button)(target));
            
            #line 22 "..\..\..\OtherWindows\BitrateWindow.xaml"
            this.BitrateSaveButton.Click += new System.Windows.RoutedEventHandler(this.BitrateSaveButton_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
