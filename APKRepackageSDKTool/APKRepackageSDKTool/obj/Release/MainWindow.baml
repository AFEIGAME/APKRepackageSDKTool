   M S B A M L   `   `   `  ���� N  JAPKRepackageSDKTool, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null9!clr-namespace:APKRepackageSDKToolAPKRepackageSDKTool  $    APKRepackageSDKTool.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     3local!clr-namespace:APKRepackageSDKTool       � Title$  APK重打包工具��$��476.859��$��865.35��.��� .+�� $��
10,10,0,38q�$
��Left=�$	��432��.+��� -    ��Name$ Text_APKPath��$��
91,12,90,0q� �� TextWrapping$ NoWrap=� J� RenderTransformOrigin$ 0.673,4.796&� �� MaxLines$ 1��$��23��$	��Top=��� $��	APK目录��$��	10,10,0,0q�$
��Left=�$��58��$��25��$	��Top=��� -   $��浏览��$��347,13,10,0q�$��19��$	��Top=��� $��SDK库目录��$��	10,40,0,0q�$
��Left=�$��71��$��25��$	��Top=�$ 0.524,-1.24&��� -   $ Text_APKExportPath��$��
91,70,90,0q�$ NoWrap=�$ 0.673,4.796&�$ 1��$��23��$	��Top=��� -   $��浏览��$��347,43,10,0q�$��19��$	��Top=�$ 
0.92,0.526&��� -    ��Name$ Button_Repack��$��	重打包��$��	0,0,10,10q�$��30��$��Bottom=�$��Right=�$��75���� $��APK导出目录��$��	10,70,0,0q�$
��Left=�$��81��$��25��$	��Top=��� -   $ Text_SDKLibPath��$��
91,42,90,0q�$ NoWrap=�$ 0.673,4.796&�$ 1��$��23��$	��Top=��� -   $��浏览��$��347,70,10,0q�$��19��$	��Top=�$ 
0.92,0.526&��� -    ��Name$ ListBox_Channel��
���   �$��1q�$��10,153,10,45q�._��� -	    ��Name$ ListBox_channel�� �� AlternationCount$
 1000������ 	 �� 
TargetType"	 ��.��� 
 �� Property
 � �� �� Value$ Stretch=����� .R�� .+��� -
   ����  �� Path$
 Name� d� 	IsChecked �� $ 	IsChecked�
 � Tag �� $
 Name��� -   $��编辑��$��Right=�$��40��$��0,0,50,0q� �� $
 Name��� -   $��删除��$��Right=�$��40�� �� $
 Name��� -   $��新增渠道��$��	0,0,90,10q�$��30��$��Bottom=�$��Right=�$��75���� $��渠道选择��$��
10,123,0,0q�$
��Left=�$	��103��$��25��$	��Top=��� $��项目选择��$��	10,98,0,0q�$
��Left=�$	��103��$��25��$	��Top=��� -    ��Name$ ComboBox_gameList��$
��Left=�$��
91,100,0,0q�$	��Top=�$	��251�� �� DisplayMemberPath$ GameName�� �� SelectedValuePath$ GameName���� -   $��编辑��$
��Left=�$��347,101,0,0q�$	��Top=�$��75���� -   $ Text_AddChannel��$��10,0,171,10q�$ NoWrap=�$ 0.673,4.796&�$ 1��$��30��$��Bottom=�� -    �Name$ Progress_repackage��$��
10,0,10,10q�$��17��$��Bottom=��� -    ��Name$ Text_progressName��$��准备��$��	10,0,10,7q�$��24��$��Bottom=�$��Center=�� $��447,20,10,38q�.+��� $��输出��$��	10,10,0,0q�$ 0.021,0.189&�$��25��$	��Top=�$
��Left=�$	��194��$��2���� -   $��清空��$��	0,10,10,0q�$��22��$	��Top=�$��Right=�$��75��$��1���� -   $ Text_output��$��
10,40,3,10q�$
 Wrap=�$��2��" �� VerticalScrollBarVisibility$
 Auto=�