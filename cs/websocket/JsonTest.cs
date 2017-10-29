using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace websocket {
    [DataContract]
    public class JSTest {
        [DataMember]
        public int intValue = 100;
        [DataMember]
        public bool boolValue = false;
        [DataMember]
        public String stringValue = "ok\nok";
        [DataMember]
        public DateTime dateTimeValue = DateTime.Now;
        [DataMember]
        public List<String> stringListValue = new List<string>();
        [DataMember]
        public IDictionary<String, String> hashValue = new Dictionary<string, string>();
        [DataMember]
        public List<JSTest2> classListValue = new List<JSTest2>();
        [DataMember]
        public IDictionary<String, JSTest2> classHashValue = new Dictionary<string, JSTest2>();
        [DataContract]
        public class JSTest2 {
            [DataMember]
            public String stringValue2 = "ok";
        }
        public JSTest() {
            this.stringListValue.Add("あ");
            this.stringListValue.Add("い");

            this.hashValue["キー"] = "ばる";

            this.classListValue.Add(new JSTest2() { stringValue2 = "あ" });
            this.classListValue.Add(new JSTest2() { stringValue2 = "い" });


            this.classHashValue["a"] = new JSTest2() { stringValue2 = "あ" };
            this.classHashValue["i"] = new JSTest2() { stringValue2 = "い" };
        }
    }
    class JsonTest {
        public static String Serialize() {
            var graph = new JSTest();
            using (var stream = new MemoryStream()) {
                DataContractJsonSerializerSettings dateFormat = new DataContractJsonSerializerSettings {
                    DateTimeFormat = new DateTimeFormat("yyyy-MM-dd hh:mm:ss.fff"),
                };
                dateFormat.UseSimpleDictionaryFormat = true;//これがないと、dictionary型が{key:"キー",value:"バリュー"}のような硬い構文になってしまう
                var serializer = new DataContractJsonSerializer(graph.GetType(), dateFormat);
                serializer.WriteObject(stream, graph);
                var json = Encoding.UTF8.GetString(stream.ToArray());
                return json;
            }
        }
    }
}
