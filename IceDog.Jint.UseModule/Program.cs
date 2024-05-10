using Jint;

namespace IceDog.Jint.Test
{
    internal class Program
    {
        static Engine CreateEngine()
        {
            // 定义引擎
            var engine = new Engine(options =>
            {
                var path = Path.Combine(AppContext.BaseDirectory, "js");
                options.EnableModules(path);
                options.LimitMemory(10_000_000);//10 mb
                options.LimitRecursion(1_000);
                options.TimeoutInterval(TimeSpan.FromSeconds(30));
                options.RegexTimeoutInterval(TimeSpan.FromSeconds(3));
                options.MaxStatements(10_000_000);
                options.MaxJsonParseDepth(10);
                options.MaxArraySize(10_000_000);
            });

            return engine;
        }

        static void Test_GlobalVariable()
        {
            // 定义引擎
            var engine = CreateEngine();
            engine.SetValue("logger", Logger.Default);

            engine.Modules.Import("./old/tool.js");
            engine.Modules.Import("./old/Person.js");
            engine.Modules.Import("./old/lodash.js");
            var code = @"
                let p1 = new Person(`tom`,20);
                logger.info(p1.introSelf());
                logger.info(p1.greet(`jerry`));

                let arr = _.map([1,2,3],x=>x*x);
                logger.info(JSON.stringify(arr));

                logger.info(`lodash version:${_.VERSION}`);
            ";
            engine.Execute(code);
            //engine.Evaluate(code);
        }

        static void Test_ESMModule()
        {
            // 定义引擎
            var engine = CreateEngine();
            engine.SetValue("logger", Logger.Default);

            Console.WriteLine("main1------------");

            var moduleMainCode = @"
                        // this import js path relative the EnableModules path
                        import Person from './esm/Person.js';
                        import * as lodash from './old/lodash.js';// just import  lodash.js,not lodash-es

                        let p1 = new Person(`tom`, 20);
                        logger.info(p1.introSelf());
                        logger.info(p1.greet(`jerry`));
                        let arr = _.map([1, 2, 3, 4], x => x * x);
                        logger.info(JSON.stringify(arr));

                        logger.info(`lodash version:${_.VERSION}`);

                        export const returnVal = 1;
            ";

            engine.Modules.Add("main1", moduleMainCode);
            var main1 = engine.Modules.Import("main1");
            var moduleMain1 = main1.AsObject();

            Console.WriteLine("main2------------");

            // we can write code to main.js directly
            var main2 = engine.Modules.Import("./esm/main.js");

            // the module should run by Get Or  As* Method，use like engine.Execute will error
            // the module main only execute once
            var moduleMain2 = main2.AsObject();
            double returnVal = main2.Get("returnVal").AsNumber();

            // this two way will thorw error
            // throw error : Unhandled exception. Esprima.ParserException: Line 3: Unexpected token
            //engine.Execute(moduleMainCode);
            //engine.Evaluate(moduleMainCode).AsNumber();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Test_GlobalVariable---------------");
            Test_GlobalVariable();
            Console.WriteLine("Test_ESMModule---------------");
            Test_ESMModule();
        }


    }
}