   M S B A M L   `   `   `  ����H  DVisualGaitLab, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullG(clr-namespace:VisualGaitLab.OtherWindowsVisualGaitLab.OtherWindows  5   	   1    +VisualGaitLab.OtherWindows.TrainingSettings   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   :local(clr-namespace:VisualGaitLab.OtherWindows     5   	     � Title$  Training Settings��5   	   $	��250��6#   $	��320��60    � Icon$ logo.png��5	   	    � WindowStartupLocation$ CenterScreen=�5
   	      WindowStyle#��[  5   	   .��5      �� .+�5   
   �� $��5,5,5,10q�6   $
��Left=�6&   $����Ideally train for at least 200,000 iterations. Global Scale scales down your frames, leave on default 0.8 unless you're experiencing training issues.��6A   $
��Wrap=�6�   �� 5   
    �� Orientation$ 
Horizontal=�6   $��5q�6.   $��Center=�69   .+�5      �� $��Global Scale: ��6   $��85��6.    �� TextAlignment$
 Left=�69     DialogTextBlockStyle#��[ 6N   �� 5      -   5         ��Name$ GlobalScaleNumberText��5      $��35��67   $	��0.8��6B   $ Center=�6M   #��[ 6d   �� 5      -   5        $��GlobalScaleSlider��5      $	��100��6.   $��Center=�6:    � Value$	 0.8Q�6�    � Minimum$
 0.01Q�6�    � Maximum$	 1.0Q�6�   	 �� IsSnapToTickEnabled	 . 
 �� TickFrequency$

 0.01Q�6�   5      �� 5   
   $ 
Horizontal=�6   $��5q�6.   $��Center=�69   .+�5      �� $��Target Iterations: ��6   $	��110��63   #��[ 6?   �� 5      -   5        $��endItersTextBox��5      $��0��6-     DialogTextBoxStyle#��[ 66   $��Center=�6   $	��100��6�    �� 	MaxLength$ 6��6�   5      �� 5   
   $ 
Horizontal=�6   $��0,30,0,0q�6.   $��Right=�6@   .+�5      �� -   5        $��CancelButton��5      $��Cancel��6)   $��5q�6:     NegativeButton#��[ 6`   �� 5      -   5        $��
SaveButton��5      $��Start��6'   $��5q�67     PositiveButton#��[ 6\   5      5!      5"      