   M S B A M L   `   `   `  ����N  JAPKRepackageSDKTool, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null9!clr-namespace:APKRepackageSDKToolAPKRepackageSDKTool  5   	   %    APKRepackageSDKTool.GameSetting   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   3local!clr-namespace:APKRepackageSDKTool     5   	     � Title$  项目列表管理��5   	   $��291.526��6   $��416.103��6)   .��5	      � .+�5
   
   �� -   5         ��Name$ ListBox_gameList��5
      $��
10,10,7,34q�6,   ��5      �� 5       �� 
TargetType" ��6   .�5      ��  �� Property � �� �� Value$ Stretch=�6C   5      ��5      �� 5      .R�5      � .+�5      �� $
��Left=�6B   ��6    ��  �� Path$ GameName�6B   �� 5      -   5        $��删除��5   !   $��Right=�6I   $��40��6e   
 � Tag 6s   �� $ GameName�5      5      5      �� 5   
   -   5        $��新增��5      $��0,0,7,10q�6   $��19��60   $��Bottom=�6T   $��Right=�6o   $��65��6�    J� RenderTransformOrigin$ 
0.529,2.21&�6�   �� 5   
   -   5         ��Name$ TextBox_GameName��5      $��
10,0,77,10q�6,   	 �� TextWrapping$
	 Wrap=�6@   $�� ��6T   $��19��6\   $��Bottom=�6h   5      5      