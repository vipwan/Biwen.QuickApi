// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎, Github: https://github.com/vipwan
// Biwen.QuickApi ,NET9+ MinimalApi CQRS
// Modify Date: 2024-09-07 16:31:34 BiwenQuickApiOptions.cs

namespace Biwen.QuickApi
{
    //using Microsoft.AspNetCore.Http;

    public class BiwenQuickApiOptions
    {
        /// <summary>
        /// 配置文件中定位Key
        /// </summary>
        public const string Key = "BiwenQuickApi:QuickApi";

        /// <summary>
        /// 全局路径前缀
        /// </summary>
        public string RoutePrefix { get; set; } = "api";

        /// <summary>
        ///默认:true 是否启动防伪令牌检测,如果启动,会自动注册中间件,请确保客户端请求时带上__RequestVerificationToken参数
        /// </summary>
        public bool EnableAntiForgeryTokens { get; set; } = true;


        /// <summary>
        /// 默认:true 是否启动发布订阅
        /// </summary>
        public bool EnablePubSub { get; set; } = true;

        /// <summary>
        /// 默认:true 是否启动ScheduleTask
        /// </summary>
        public bool EnableScheduling { get; set; } = true;

        /// <summary>
        /// 使用QuickApi的方式返回规范化异常 默认:false,如果:false 仅返回:500
        /// </summary>
        public bool UseQuickApiExceptionResultBuilder { get; set; } = false;


    }
}