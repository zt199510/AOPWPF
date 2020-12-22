MVVMPageLib

### 介绍

让 wpf 更便捷

### 特点

1.ioc（使用的 autofac） 2.自动捕捉类中的 icommand
3.icommand 可以做 aop 4.分装了一个简单的切页模型

### 起步

#### 1.创建 wpf 项目

#### 2.在 app.xml 中移除 StartupUri;

#### 3.创建一个 startup.cs 的类，这个类是用来注册的.

```
    public class TestStartup : MVVMPageLib.StartUp
    {
        public override ContainerBuilder ConfigureServices(ContainerBuilder servercollection)
        {
            servercollection.AddMVVMPage();//这个方法必须要使用
            return servercollection;
        }
    }
```

#### 4.进入 app.cs 添加如下是代码

```
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            new MvvmPageBuilder()
                .SetStartUp<TestStartup>()//初始化ioc要用的类
                .EnableCatchException(error => Console.WriteLine(error))//这个是全局异常捕捉
                .EnableSingleProgram()//这个是让程序在一台电脑上只能运行一个
                .Build()//创建
                .Run();//跑起来
        }
    }

```

Run 方法是个重载，其中一个是这样子的

```
    public void Run(Action<Autofac.ILifetimeScope, IMVVMWindowModel> action)
```

    这个委托第一个参数是当前程序的ioc更容器，第二个参数是IMVVMWindowModel，这个类型是在上面的```servercollection.AddMVVMPage()```添加的

还有一个重载是什么参数都不传的，他会默认给你启动窗体，启动的窗体通过 IMVVMWindowModel 来获取

#### 5.创建一个类，实现 IMVVMWindowModel，继承 BaseMvvmModel

```
public class MainWindow : BaseMvvmModel, IMVVMWindowModel
    {
        private Window window;
        public Window Window => window ??= new MainWindow { DataContext = this };//这个用来指定你要用什么窗体启动

        public ILifetimeScope ServiceProvider { get; set; }//这个是ioc根容器

        public Task ClosedModel(BasePageModel closedmodel)//这个是关闭tab的方法
        {
            throw new NotImplementedException();
        }

        public Task InvokedModel(BasePageModel invokedmodel)//这个是开启tab的方法
        {
            throw new NotImplementedException();
        }
    }
```

这个时候程序就可以跑起来了，他会启动 Window 窗体。

### 自动捕捉 command

#### 捕捉 Icommand

凡是继承了 BaseMvvmModel 的类，都会在构造函数里面反射当前类下所有返回 IActionResult 或者 Task<IActionResult>的方法，
把他通过 AopCommand 构造出来存到一个私有的字典里面，待 wpf 拿。
在之前创建的 model 和 xaml 里面分别添加

```
       public IActionResult Test() {
            HandyControl.Controls.MessageBox.Show("sdfsdf");
            return OK();
        }
        <Button Content="test" Command="{Binding Test}"/>
```

这个时候点击这个 button 就会执行这个 Test 方法了

在自己的 wpf 项目中添加类继承 StartUp。
实现里面的方法。

#### 修改 icommand 的属性

##### Enable 属性

    在构造BaseMvvmModel的时候，会把生成的command都放到一个字典里面，并且因为这个类实现了索引器，我们就可以通过[name]在取值了,
    给model添加一个构造函数，并且添加如下代码

```
  public MainWindowModel() {
            base[nameof(Test)].Enable = false;
        }
```

    这个时候再去运行就会发现按钮被禁用了。
    除了手动赋值enable，我们还可以给他放一个委托

```
     base[nameof(Test)].Enablefunc =obj=> false;
```

    再在合适的地方调用这个方法

```
    base[nameof(Test)].EnableChanged();
```

##### command 的 aop

    command的类型是AOPCommand ，他有如下几个参数
    1. BaseMvvmModel model 这个是当前command所处于的model
    2. MethodInfo commandmethod 这个是command要执行方法的信息
    3. Func<object, Task> action 这个是触发command的时候执行的方法。默认情况下，这个方法执行的代码是当前model类中同名的方法
    我们可以在构造函数中遍历方法，添加自己的逻辑，代码如下

```
    foreach (var item in base.Keys)
            {
                base.Keys[item.Key] = new AOPCommand(item.Value.Model, item.Value.Command,
                        async obj =>
                        {
                            HandyControl.Controls.MessageBox.Show("before");
                            await item.Value.action(obj);
                            HandyControl.Controls.MessageBox.Show("after");
                        }
                    );
            }
```

    这个时候再去点击。就会弹出三个窗口

### 更新属性

#### 参与贡献

1. Fork 本仓库
2. 新建 Feat_xxx 分支
3. 提交代码
4. 新建 Pull Request
