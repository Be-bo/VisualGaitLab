   M S B A M L   `   `   `  ����H  DVisualGaitLab, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullG(clr-namespace:VisualGaitLab.OtherWindowsVisualGaitLab.OtherWindows  5   	   .    (VisualGaitLab.OtherWindows.BitrateWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   :local(clr-namespace:VisualGaitLab.OtherWindows     5   	     � Title$  Resolution Reduction��5   	   $	��380��6&   $	��400��63    � Icon$ logo.png��5	   	    � WindowStartupLocation$ CenterScreen=�5
   	      WindowStyle#��[  5   	   .��5      �� .+�5   
   �� $��5q�6   $
��Left=�6   $����Reducing video resolution (as well as cropping the video) can improve reliability. It's especially relevant if you have a less powerful PC.

WARNING: Resolution reduction will distort your video if its aspect ratio isn't 16:9 (choose No Reduction if that's the case).��6:   $
��Wrap=�6U  �� 5   
    �� Orientation$ Vertical=�6   $��10q�6,   $��Center=�68   .+�5      	� -   5        $��Import1080RadioButton��5   %   $��5q�6    	� 	GroupName$ ImportResolution��6B   $��1080p��6_    d� 	IsChecked$ FalseY�6o   	� 5      -   5        $��Import720RadioButton��5   %   $��5q�6   $ ImportResolution��6A   $
��720p��6^   $
 TrueY�6m   	� 5      -   5        $��Import480RadioButton��5   %   $��5q�6   $ ImportResolution��6A   $
��480p��6^   $ FalseY�6m   	� 5      -   5        $��Import360RadioButton��5   %   $��5q�6   $ ImportResolution��6A   $
��360p��6^   $ FalseY�6m   	� 5      -   5        $��ImportNothingRadioButton��5   %   $��5q�6   $ ImportResolution��6E   $��No Reduction��6b   $ FalseY�6y   5      �� 5   
   $ 
Horizontal=�6   $��0,10,0,0q�6.   $��Right=�6@   .+�5      �� -   5        $��BitrateSaveButton��5      $��Confirm��6.   $��5q�6`     PositiveButton#��[ 6k   5      5      5      