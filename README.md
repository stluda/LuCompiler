## LuCompiler（lu语言编译器）



### 这是什么？

LuCompiler，是我为自己设计的语言——**lu语言**编写的编译器，能够将lu语言编译成vbs脚本语言。（lu语言专门为了找图类自动脚本而设计的语言）



### 关于找图类自动脚本

找图类自动脚本可以完成一些无人值守的自动化操作，比如说游戏挂机，或QQ微信抢红包等。


拿QQ抢红包为例，如果想编写找图自动脚本自动抢红包的话该怎么做呢？流程大致可以分为以下3步：
1. 在别人发红包时，截图取样本。
2. 提取红包的关键图片，关键图片需满足：拥有一类被找图像有且仅有的共通特征。
3. 编写找图脚本，逻辑是循环判断一定屏幕范围内，是否找到关键图片，如果找到的话则点击图片所在位置，即可实现无人值守自动抢红包的功能。

   ![](img/red.png)




### 关于lu语言

本人曾经有一段时间喜欢写游戏的挂机脚本，而当时使用的按键精灵的运行语言为vbs语言，用原生IDE实现找图功能的代码较为繁琐。拿上面的自动抢红包举例，实际的编写流程如下：



1. 首先，我们知道红包肯定是在聊天窗口内部的。所以我们第一步是要在整个屏幕中查找聊天窗口的位置，而为了确定聊天窗口的位置，**我们需要提取聊天的关键图片**，我们假设这个关键图片是1.png。

2. **我们需要测量聊天窗口的宽高**，这样一来，一旦找到聊天窗口的位置，我们就可以缩小查找范围，只在聊天窗口内部进行查找，以提高查找效率。**我们还需要测量关键图距离聊天窗口左上角的偏移量**，这样一来才能通过聊天窗口关键图的位置来推断整个聊天的位置。

3. **我们需要提取红包的关键图片**，我们假设这个关键图片是2.png。到了这一步，所需要的数据都有了：

   ![](img/pos.png)

4. 开始编写按键精灵的运行语言，vbs语言的脚本：

   ```vbscript
   Dim msgwindow_width,msgwindow_height //聊天窗口的宽高
   Dim msgwindow_offsetX,msgwindow_offsetY //聊天窗口左上角距离关键图的偏移量
   msgwindow_width = 394
   msgwindow_height = 592
   msgwindow_offsetX = 2 - 303
   msgwindow_offsetY = 40 - 45
   
   //循环
   Do 
       //先查找聊天窗口的位置，查找聊天窗口的关键图
       XY = FindPic(0, 0, 1024, 768, "1.png", 1, 0)
       ZB = InStr(XY, "|")
       X = Clng(Left(XY, ZB - 1)): Y = Clng(Right(XY, Len(XY) - ZB))//找到的X,Y的坐标
       
       If X > 0 And Y > 0 Then //若找到聊天窗口
           //定位聊天窗口，(X1,Y1)、(X2,Y2)分别是聊天窗口的左上角和右上角
           X1 = X + msgwindow_offsetX
           Y1 = Y + msgwindow_offsetY
           X2 = X1 + msgwindow_width
           Y2 = X2 + msgwindow_height
           
           //在聊天窗口范围内，查找红包的关键图
           XY = FindPic(X1, Y1, X2, Y2, "2.png", 1, 0)
           ZB = InStr(XY, "|")
           X = Clng(Left(XY, ZB - 1)): Y = Clng(Right(XY, Len(XY) - ZB))
           
           //点击红包关键图的位置，即点击红包
           Click(X,Y)      
           
       Endif    
       //延迟100毫秒
       Delay 100
   Wend
   ```



看上去很复杂，而且可读性很差，对吧？ 这是因为我们写脚本大部分代码和时间都浪费在坐标计算，繁琐的找图语句上面了。

而如果我们试着用自然语言的伪代码描述一下我们做了什么，会怎样呢：

```
循环
{
    找聊天窗口关键图。
    如果找到，则在聊天窗口内部找红包的关键图。
    如果找到，则点击关键图。    
    延迟100秒。
}
```



我们会发现，其实整个脚本逻辑其实非常简单。而实际代码实现时，因为找图的过程比较繁琐，会涉及坐标的确定以及计算，所以让整个代码看上去很臃肿。而这仅仅只是一个点击红包的小脚本。如果我们需要编写复杂的大型脚本，其可读性和维护性可想而知。 



所以我在想，有没有什么办法能将图片坐标计算和脚本逻辑分离开来，让脚本编写者更专注于程序逻辑本身，而不是这些繁琐的细节？ **lu语言，就是为了解决这个问题而产生的。**



如果用lu语言实现同样的功能的话，代码会是这样的：

```
while(true)
{
    if<全域_聊天窗口>{//如果在全域内找到聊天窗口。全域是指整个屏幕范围
        fclickE<聊天窗口_红包>;//则在聊天窗口内寻找红包，若找到，则点击它
    }
    delay(100);
}
```

我们可以看到，lu语言把找图，点击图这个行为抽象化了。我们不再去关心找到的图的坐标是什么位置，不再去关心需要点击的位置是哪里，我们只知道我们要去点击红包，而这个红包在聊天窗口中，这就足够了。



当然，只是提供lu脚本给编译器是不够的，因为缺少坐标信息，因此还要额外准备一个xml文件，里面记录了需要用到的信息：

```xml
<Area Name="all" CHName="全域" Sim="0.8" OffsetColor="#101010" DoesUseLastFoundPos="False" IsFindable="True" FPoint="0,0" IsRefersParent="False" SearchMode="FixedInArea" Range="0,0,0,0" KeyRange="0,0,0,0" Mixcode="N12N74XV6L" OffsetPos="0,0" Width="0" Height="0" KeyWidth="0" KeyHeight="0">
    <Element Name="ltck" CHName="聊天窗口" Sim="0.8" OffsetColor="#101010" DoesUseLastFoundPos="False" IsFindable="True" FPoint="307,42" IsRefersParent="False" SearchMode="FixedInArea" Range="2,40,394,592" KeyRange="307,42,88,38" Mixcode="F0HI05N3BJ" OffsetPos="-108,294" />
    <Element Name="hb" CHName="红包的关键图" Sim="0.8" OffsetColor="#101010" DoesUseLastFoundPos="False" IsFindable="True" FPoint="213,534" IsRefersParent="False" SearchMode="FixedInArea" Range="213,534,86,25" KeyRange="213,534,86,25" Mixcode="XK8NM6Q7SP" OffsetPos="43,12" />
    <Element Name="ltckgjt" CHName="聊天窗口的关键图" Sim="0.8" OffsetColor="#101010" DoesUseLastFoundPos="False" IsFindable="True" FPoint="303,45" IsRefersParent="False" SearchMode="FixedInArea" Range="303,45,91,32" KeyRange="303,45,91,32" Mixcode="V8KFXN6HE7" OffsetPos="45,16" />
</Area>
```

这个xml文件是由我编写了另外的图像工具自动生成的，编译器只要分析这些信息，就能得知各个图片元素的位置关系，从而实现找图行为的抽象化。



用这样2个文件，我们就分离了繁杂的坐标计算和找图逻辑。xml文件负责保存需要用到的各种数据，而lu脚本则只关心我们要寻找的目标，要点击的目标，而不关心目标的具体位置。如此一来，整个找图脚本的逻辑会变得非常清晰，大大增强可读性和可维护性。



### lu语言语法

待续