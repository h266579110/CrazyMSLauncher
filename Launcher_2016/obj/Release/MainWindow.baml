   M S B A M L   `   `   `  ����   Launcher    Launcher_2016.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Aav2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     -     � Title$  MapleCustom Launcher��$	��677��$	��704�� � WindowStyle$
 None=� � 
ResizeMode$ NoResize=� J� 	AllowDrop . $��Center=�$��Center=� � AllowsTransparency .  � WindowStartupLocation$ CenterScreen=� J� Opacity �� *Value+ �� ElementName$ _opacitySlider������  �� ImageSource$ /launcher_custom.png��.��� 	 � ColumnDefinitions	 �� $	��98* ��� $��2* ��� $
��220* ��� $
��362* �.+�� -   
 �Name$
 pbStatus��$��4��$��23��$
��Left=�$��41,601,0,14q�$	��280�����	#FFF85C19$
 00.1Q����	#FF1B1B1B$��0q�$y�Visible=�����  �� EndPoint$ 1,0.5&� �� 
StartPoint$ 0,0.5&�.���  � Color$ Black�� � Offset$ 0Q�� $ White��$ 1Q� J� OpacityMask �� $ 1,0.5&�$ 0,0.5&�.��� $ Black��$ 0Q�� $ White��$ 1Q��� -    ��Name$ button1��   MyButton#��[  $��3��$��54��$
��Left=�$��122,612,0,0q�$	��Top=�$	��206��
������ $y�Visible=�$ 1Q�$��0q�$��1q�|�.  � DataContext �� ���� $ 	/play.png���� -    ��Name$ label1��$��狀態:��$��2��$��22��$
��Left=�$��
40,601,0,0q�$	��Top=�$��63��
�������$��10(��� -   $ label2��$��等待開始...��$��1��$��22��$
��Left=�$��	2,601,0,0q�$	��Top=�$	��150��
�������$��10(�$��2���� -   $ label3��$��/��$��2��$��22��$
��Left=�$��181,601,0,0q�$	��Top=�$��25�����	#FFEFEFEF$��10(��� -   $ label4��$��0��$��2��$��22��$
��Left=�$��164,601,0,0q�$	��Top=�$��25�����	#FFF5E9E9$��10(��� -   $ label5��$�� ��$��4��$��27��$
��Left=�$��	62,87,0,0q�$	��Top=�$	��290��
��������� -	   $ label6��$��0��$��2��$��22��$
��Left=�$��194,601,0,0q�$	��Top=�$��25�����	#FFFFEFEF$��10(��� -
   $ label7��$��1��$��23��$
��Left=�$��	2,645,0,0q�$	��Top=�$	��125�����	#FFF8F8F8$��0 MB / 0 MB��$��10(�$��2���� -   $ label8��$��下載進度:��$��2��$��23��$
��Left=�$��
40,645,0,0q�$	��Top=�$��63��
�������$��10(��� $��3��$��35,615,292,43q���Z�  Z� �� -    ��Name$ ws��$�� ?����	#FFCACC00 �� TextDecorations Z� .��� $�官方網站���� $��3��$��37,644,290,13q��� -   $
 disc��$�� ?����#5865F2 Z� .��� $�Discord���� -   $��3��$�� ��$
��Left=�$��20��$��
313,90,0,0q�$	��Top=�$��22����Z� ����  �� 
TargetType" �� �� BasedOn Z� .���  �� Property � �� �� Value ��  �� 
TargetType" ��.R��� #��z��._��� $��Center=�$��Center=����� $ /close_window_96px.png���� -    ��Name$ resComboBox��$��2��$��125,646,9,0q�$��20��$	��Top=�.@��� $��1024*768���� $��	1280*1024���� $��1280*720���� $��1366*768���� $��1600*900���� $��	1920*1080��