# TeamFlow
基于Unity和GPT的工作流可视化编辑器

## 简介 Intro

近几年来，诸如GPT这样的大型语言模型（LLM）应用变得越来越广泛。最基础、稳定的应用场景如下：

- 了解未知的领域，扩充个人知识库
- 润色、修改、翻译文章
- 提取文本信息、总结文本内容
- ......

这些都是利用了语言模型最基础的能力--文字处理能力的案例，然而，GPT这样的AI的火热显然表明其能力不止于此，在近一年的发展和变化过程中，我们见到的应用场景有：

- 编程（Copilot）
- 绘图（GPT+DALLE）
- 视频总结
- ......

对于简单的应用场景，通常我们打开网页端的GPT就能做到。然而，对于一些复杂的应用场景，往往需要复杂的提示工程设计。

一言以蔽之，就是将GPT这样的AI，真正嵌入到我们日常的工作流中。做得比较好的有NotionAI、Copilot，但由于其黑盒的特征，我们看不到内部的工作流程，或许都不只是由一个AI所能完成的，是通过调度多个AI，通过Prompt编写、RAG检索增强生成技术、Embedding技术甚至Finetuning技术等一系列的技术栈完成。而其实这些当然不是通过调用网页端的聊天版GPT完成的，需要使用OpenAI所提供的api完成。

本项目，TeamFlow系统，基于Unity和XNode制作了一个可视化的AI工作流编辑器，希望能以节点的形式简化api的使用，使得人人都能将自己觉得好用的工作流保存下来；对于开发者而言，希望能提供一个较好的测试和扩展的开发环境，最终目标是可以部署和发布自定义的GPT。

## 应用示意图

比如设计两个AI，一个用于整理游戏开发的需求，一个用于编写具体的代码。

![image-20240306165945358](C:\Users\57862\AppData\Roaming\Typora\typora-user-images\image-20240306165945358.png)

![image-20240306170116174](C:\Users\57862\AppData\Roaming\Typora\typora-user-images\image-20240306170116174.png)

## 部署和安装

本项目依赖于XNode、QFramework、Unitask以及Odin等插件，除了Odin插件需要自行去安装最新版本之外，其它均已上传在github仓库中。

XNode和QFramework的部分代码，基于TeamFlow系统的需求有所改动，所以必须下载本项目中的版本！

### 安装Odin

编辑器利用了Odin的很多特性，请下载并安装Odin Inspector，官方网站：

https://odininspector.com/

### OpenAI api的配置

1、在Project窗口中的Resources文件夹下，右键创建一个OpenAIConfiguration

![image-20240306171114489](C:\Users\57862\AppData\Roaming\Typora\typora-user-images\image-20240306171114489.png)

2、在该资源对应的Inspector视图中，填写OpenAI的key以及organization id。

![image-20240306171253092](C:\Users\57862\AppData\Roaming\Typora\typora-user-images\image-20240306171253092.png)

### 快速入门

1、在Scripts/TeamFlow/Data/Resources文件夹下，有名为AssistantData和FileData的SO文件，如果没有，请通过右键-Create Asset-TeamFlow菜单创建一个。

![image-20240306171900770](C:\Users\57862\AppData\Roaming\Typora\typora-user-images\image-20240306171900770.png)

![image-20240306172107020](C:\Users\57862\AppData\Roaming\Typora\typora-user-images\image-20240306172107020.png)

2、在第1步中所示的同样菜单下，有一个TeamFlow Graph的选项，可以点击创建在自己喜欢的地方。

3、双击创建出的资源，可打开工作流的可视化编辑界面

![image-20240306172331499](C:\Users\57862\AppData\Roaming\Typora\typora-user-images\image-20240306172331499.png)

4、在XNode的画布中，右键选择创建节点后，可以看到可创建的所有类型节点的目录，并支持搜索功能

![image-20240306172446378](C:\Users\57862\AppData\Roaming\Typora\typora-user-images\image-20240306172446378.png)

5、如下图所示，可以创建出对应的节点，过程中，一些助手可能需要自己创建，因为这与你自己的OpenAI后台的数据是同步的，我这里有这些助手是因为已经存在于OpenAI服务器中。

![image-20240306172541261](C:\Users\57862\AppData\Roaming\Typora\typora-user-images\image-20240306172541261.png)

6、全部完成后，点击初始化工作流，再点击开始运行即可运行本工作流。

7、通过Unity的Console窗口可以看到Debug数据，推荐的方式是通过显示结果节点接收结果。其利用了一个内置的Markdown渲染器，能够较为整洁的显示出最终结果。你可以通过提示词的形式，让AI以Markdown格式回复。

## 节点介绍（施工中）
