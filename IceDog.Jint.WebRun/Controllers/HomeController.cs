using IceDog.Jint.WebRun.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Jint;
using Jint.Runtime;
using System.Text.Json;

namespace IceDog.Jint.WebRun.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private static string _lodashCode = System.IO.File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "wwwroot", "esm-lib", "lodash.js"));

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static Engine CreateEngine()
        {
            // 定义引擎
            var engine = new Engine(options =>
            {
                options.LimitMemory(10_000_000_000);
                options.LimitRecursion(2_000);
                options.TimeoutInterval(TimeSpan.FromSeconds(30));
                options.RegexTimeoutInterval(TimeSpan.FromSeconds(3));
                options.MaxStatements(100_000_000);
                options.MaxJsonParseDepth(10);
                options.MaxArraySize(10_000_000);
            });

            return engine;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult ExecTool()
        {
            return View();
        }

        /// <summary>
        /// 执行插件工具函数
        /// </summary>
        /// <param name="code"></param>
        /// <param name="paramObjJson"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ExecTool([FromForm] string code, [FromForm] string paramObjJson)
        {

            var engine = CreateEngine();

            engine.SetValue("dbexec", (string sql) =>
            {
                return new Dictionary<string, object>()
                {
                    {"result","ok" },
                    {"sql",sql }
                };
            });

            engine.SetValue("paramObjJson", paramObjJson);

            engine.Modules.Add("logger", @"
            const log=(type,message)=>{};
            const logger={
                log,
                debug(message){
                    log(`debug`,message)
                },
                info(message){
                    log(`info`,message)
                },
                warn(message){
                    log(`warn`,message)
                },
                error(message){
                    log(`error`,message)
                }
            };
            export default logger;
            ");

            engine.Modules.Add("db", @"
                const query=(sql)=>{
                        dbexec(sql);
                };

                export {query};
            ");

            engine.Modules.Add("lodash", _lodashCode);

            engine.Modules.Add("handler", code);

            engine.Modules.Add("main", $@"
             import logger from 'logger';
             import {{handler}} from 'handler';

             let input=JSON.parse(paramObjJson);

             export const result = handler({{input,logger}});
            ");
            try
            {
                var modMain = engine.Modules.Import("main");
                var result = modMain.Get("result").UnwrapIfPromise().ToObject();
                return Json(result);
            }
            catch (JavaScriptException jsEx)
            {
                return Json(new { msg = jsEx.Message });
            }
            catch (PromiseRejectedException prEx)
            {
                return Json(new { msg = prEx.RejectedValue.ToString() });
            }
            catch (Exception ex)
            {

                return Json(new { msg = ex.Message });
            }


        }

        [HttpGet]
        public IActionResult DebugTool()
        {
            return View();
        }

        private static readonly string _dateFormat = "HH:mm:ss";
        private string getDateNowText() => DateTime.Now.ToString(_dateFormat);

        private async Task WriteEventSourceLogData(string logType, string message)
        {
            var data = $"[{getDateNowText()}][{logType}] {message}";
            var obj = new
            {
                data = data,
                type = "log"
            };
            await Response.WriteAsync($"data:{JsonSerializer.Serialize(obj)}\n\n");
        }

        private async Task WriteEventSourceResultData(string data)
        {
            var obj = new
            {
                data = data,
                type = "result"
            };
            await Response.WriteAsync($"data:{JsonSerializer.Serialize(obj)}\n\n");
        }

        private async Task WriteEventSourceCloseData()
        {
            var obj = new
            {
                data = "close",
                type = "close"
            };
            await Response.WriteAsync($"data:{JsonSerializer.Serialize(obj)}\n\n");
        }

        /// <summary>
        /// 调试插件工具函数
        /// </summary>
        /// <param name="code"></param>
        /// <param name="paramObjJson"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task DebugToolEventSource([FromQuery] string code, [FromQuery] string paramObjJson)
        {
            Stopwatch stopwatch1 = Stopwatch.StartNew();
            Response.ContentType = "text/event-stream;charset=utf-8;";
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");

            bool isExecEnd = false;
            _ = Task.Factory.StartNew(async () =>
            {
                var engine = CreateEngine();
                await WriteEventSourceLogData("info", "初始化中...");
                engine.SetValue("log", WriteEventSourceLogData);

                engine.SetValue("dbexec", (string sql) =>
                {
                    Console.WriteLine(sql);
                });

                engine.SetValue("paramObjJson", paramObjJson);

                engine.Modules.Add("logger", @"
                const logger={
                    log,
                    debug(message){
                        log(`debug`,message)
                    },
                    info(message){
                        log(`info`,message)
                    },
                    warn(message){
                        log(`warn`,message)
                    },
                    error(message){
                        log(`error`,message)
                    }
                };
                export default logger;
                ");

                engine.Modules.Add("db", @"
                    const query=(sql)=>{
                            dbexec(sql);
                    };

                    export {query};
                ");

                engine.Modules.Add("lodash", _lodashCode);

                engine.Modules.Add("handler", code);

                engine.Modules.Add("main", $@"
                 import logger from 'logger';
                 import {{handler}} from 'handler';

                 let input=JSON.parse(paramObjJson);

                 export const result = handler({{input,logger}});
                ");
                try
                {
                    await WriteEventSourceLogData("info", "函数执行中...");
                    var modMain = engine.Modules.Import("main");
                    var result = modMain.Get("result").UnwrapIfPromise().ToObject();
                    await WriteEventSourceLogData("info", "函数执行完毕");
                    await WriteEventSourceResultData(JsonSerializer.Serialize(result));
                }
                catch (JavaScriptException jsEx)
                {
                    await WriteEventSourceLogData("error", $"函数执行出错:{jsEx.Message}");
                }
                catch (PromiseRejectedException prEx)
                {
                    await WriteEventSourceLogData("error", $"函数执行出错:{prEx.RejectedValue}");
                }
                catch (Exception ex)
                {
                    await WriteEventSourceLogData("error", $"函数执行出错:{ex.Message}");
                }
                finally
                {
                    isExecEnd = true;
                }
            });

            while (isExecEnd == false)
            {
                await Task.Delay(100);
            }
            stopwatch1.Stop();
            var actionCostTime = stopwatch1.ElapsedMilliseconds;
            await WriteEventSourceLogData("info", $"执行成功,函数耗时{actionCostTime}ms");
            await WriteEventSourceCloseData();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}