using Flurl.Util;
using KanonBot.Drivers;
using KanonBot.functions;
using KanonBot.functions.osu;
using KanonBot.functions.osubot;
using KanonBot.Message;
using LanguageExt;
using Serilog;

namespace KanonBot.command_parser
{
    public static class Universal
    {
        public static async Task Parser(Target target)
        {
            // 解析之前先确认是否有等待的消息
            foreach (var (t, cw) in Target.Waiters.Value)
            {
                if (t.platform == target.platform && t.sender == target.sender)
                {
                    await cw.WriteAsync(target);
                    return;
                }
            }

            string? cmd = null;
            var msg = target.msg;

            // var isAtSelf = false;
            // if (msg.StartsWith(new Message.AtSegment(target.selfAccount!, target.platform)))
            // {
            //     isAtSelf = true;
            //     msg = Message.Chain.FromList(msg.ToList().Slice(1, msg.Length()));
            // }

            // var seg = msg.Find<ImageSegment>();
            // if (seg != null) {
            //     if (seg.t is ImageSegment.Type.Url) {
            //         var imageUrl = seg.value;
            //     }
            // }


            if (msg.StartsWith("!") || msg.StartsWith("/") || msg.StartsWith("！"))
            {
                cmd = msg.Build();
                cmd = cmd.Substring(1); //删除命令唤起符
            }

            if (cmd != null)
            {
                //cmd = cmd.ToLower(); //转小写
                cmd = Utils.ToDBC(cmd); //转半角
                // cmd = Utils.ParseAt(cmd);

                // 有些例外，用StartsWith匹配
                if (cmd.StartsWith("bp"))
                {
                    if (cmd.StartsWith("bpme"))
                        return;
                    await BestPerformance.Execute(target, cmd[2..].Trim());
                    return;
                }

                string rootCmd,
                    childCmd = "";
                try
                {
                    var tmp = cmd.SplitOnFirstOccurence(" ");
                    rootCmd = tmp[0].Trim();
                    childCmd = tmp[1].Trim();
                }
                catch
                {
                    rootCmd = cmd;
                }

                //Console.WriteLine(target.account + target.platform);
                try
                {
                    switch (rootCmd.ToLower())
                    {
                        case "reg":
                            await Accounts.RegAccount(target, childCmd);
                            return;
                        case "bind":
                            await Accounts.BindService(target, childCmd);
                            return;
                        case "info":
                            await Info.Execute(target, childCmd);
                            return;
                        case "recent":
                            await Recent.Execute(target, childCmd, true);
                            return;
                        case "re":
                            await Recent.Execute(target, childCmd, true);
                            return;
                        case "pr":
                            await Recent.Execute(target, childCmd, false);
                            return;
                        case "bp":
                            await BestPerformance.Execute(target, childCmd);
                            return;
                        case "score":
                            await Score.Execute(target, childCmd);
                            return;
                        case "help":
                            await Help.Execute(target, childCmd);
                            return;
                        case "ping":
                            await Ping.Execute(target, childCmd);
                            return;
                        case "update":
                            await Update.Execute(target, childCmd);
                            return;
                        case "get":
                            await Get.Execute(target, childCmd);
                            return; // get bonuspp/elo/rolecost/bpht/todaybp/annualpass
                        case "badge":
                            await Badge.Execute(target, childCmd);
                            return;
                        case "leeway":
                        case "lc":
                            await Leeway.Execute(target, childCmd);
                            return;
                        case "set":
                            await Setter.Execute(target, childCmd);
                            return;
                        case "ppvs":
                            await PPvs.Execute(target, childCmd);
                            return;

                        // Admin
                        case "sudo": //管理员
                            await Sudo.Execute(target, childCmd);
                            return;
                        case "su": //超级管理员
                            await Su.Execute(target, childCmd);
                            return;
                        case "dailyupdate":
                            return;
                        default:
                            return;
                    }
                }
                catch (Flurl.Http.FlurlHttpTimeoutException)
                {
                    await target.reply("API访问超时，请稍后重试吧");
                }
                catch (Flurl.Http.FlurlHttpException ex)
                {
                    await target.reply("网络出现错误！错误已上报");
                    var rtmp = $"Target Message: {target.msg}\n" + $"Exception: {ex}\n";
                    // $"Message: {ex.Message}\n" +
                    // $"Source: {ex.Source}\n" +
                    // $"StackTrace: {ex.StackTrace}";
                    Utils.SendDebugMail("mono@desu.life", rtmp);
                    Utils.SendDebugMail("fantasyzhjk@qq.com", rtmp);
                    Log.Error("网络异常 ↓\n{ex}", ex);
                }
                catch (System.IO.IOException ex)
                {
                    // 文件竞争问题, 懒得处理了直接摆烂
                    Log.Error("出现文件竞争问题 ↓\n{ex}", ex);
                    if (ex.Message.Contains("being used by another process"))
                        return;
                }
                catch (Exception ex)
                {
                    await target.reply("出现了未知错误，错误内容已自动上报");
                    var rtmp = $"Target Message: {target.msg}\n" + $"Exception: {ex}\n";
                    // $"Message: {ex.Message}\n" +
                    // $"Source: {ex.Source}\n" +
                    // $"StackTrace: {ex.StackTrace}";
                    Utils.SendDebugMail("mono@desu.life", rtmp);
                    Utils.SendDebugMail("fantasyzhjk@qq.com", rtmp);
                    Log.Error("执行指令异常 ↓\n{ex}", ex);
                }
            }
            else
            {
                return;
            }
        }
    }
}
