using System;
using System.Text;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace Web.SurveySystem.Helpers
{
    public class JsonNetResult : ActionResult
    {
        public JsonNetResult()
        {
            this.SerializerSettings = new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore};
            this.Formatting = Formatting.None;
            this.StatusCode = 200;
        }
        public JsonNetResult(object data, bool useDateConverter = true) : this()
        {
            this.Data = data;
            this.UseDateConverter = useDateConverter;
        }
        public JsonNetResult(object data, string contentType, bool useDateConverter = true) : this(data, useDateConverter)
        {
            this.ContentType = contentType;
        }

        public static JsonNetResult Success => new JsonNetResult(new {success = true});

        public Encoding ContentEncoding { get; set; }
        public string ContentType { get; set; }
        public object Data { get; set; }
        public int StatusCode { get; set; }
        public JsonSerializerSettings SerializerSettings { get; set; }
        public Formatting Formatting { get; set; }
        public bool UseDateConverter { get; set; }
        public string DateFormat { get; set; }

        public static JsonNetResult SuccessMessage(string message)
        {
            return new JsonNetResult(new {success = true, message = message, responseText = message});
        }

        public static JsonNetResult Failure(string message)
        {
            return new JsonNetResult(new {success = false, message = message, responseText = message})
            {
                StatusCode = 500
            };
        }

        public static JsonNetResult Warn(string message)
        {
            return new JsonNetResult(new {success = false, message = message, responseText = message});
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            // Для получения даты в формате new Date (1455281834669) используется конвертер Newtonsoft, 
            // что соответсвует this.UseDateConverter и useJSDateTime значению true, 
            // иначе получим дату в формате "2016-02-12T15:56:08.935+03:00"
            var useJSDateTime = this.UseDateConverter;

            if (context.RequestContext != null && context.RequestContext.HttpContext != null && context.RequestContext.HttpContext.Request != null)
            {
                this.DateFormat = context.RequestContext.HttpContext.Request.QueryString.Get("dateformat");
            }

            var response = context.HttpContext.Response;
            try
            {
                response.StatusCode = this.StatusCode;
            }
            catch
            {
            }

            response.ContentType = !string.IsNullOrEmpty(this.ContentType) ? this.ContentType : "application/json";
            if (this.ContentEncoding != null)
            {
                response.ContentEncoding = this.ContentEncoding;
            }
            if (this.Data == null)
            {
                return;
            }

            var writer = new JsonTextWriter(response.Output) {Formatting = this.Formatting};
            var serializer = JsonSerializer.Create(this.SerializerSettings);
            serializer.Serialize(writer, this.Data);
            writer.Flush();
        }
    }
}