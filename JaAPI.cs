using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Xml;
using System.Data;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace JaAPI
{

    public struct PhoneNumberStruct
    {
        public string phoneNumber;
        public string type;
    }

    public struct ItemsStruct
    {
        public string type;
        public string name;
        public string firstName;
        public string middleName;
        public string lastName;
        public string address;
        public string occupation;
        public string postalCode;
        public string postalStation;
        public PhoneNumberStruct phoneNumber;
        public List<PhoneNumberStruct> additionalPhonenumbers;
        public string nationalIdNumber;
        public string phonebookIdNumber;
        public string solicitationProhibited;
        public string hasCoordinates;
        public string rangeMatch;
        public string url;
        public string email;
        public string faxNumber;
    }

    public struct JaStruct
    {
        public string error;
        public string query;
        public string copyright;
        public string firstItem;
        public string lastItem;
        public string totalResults;
        public string itemsPerPage;
        public string searchTime;
        public string totalTime;
        public string apiVersion;
        public List<ItemsStruct> items;
    }

    public class Ja_API
    {


        private static void UpdateItem(object o, string field_name, string value)
        {
            FieldInfo field = typeof(ItemsStruct).GetField(field_name);
            field.SetValue(o, value);
        }

        private static void UpdatePhoneNumber(object o, PhoneNumberStruct value)
        {
            FieldInfo field = typeof(ItemsStruct).GetField("phoneNumber");
            field.SetValue(o, value);
        }

        private static void UpdateAddPhoneNumber(object o, List<PhoneNumberStruct> value)
        {
            FieldInfo field = typeof(ItemsStruct).GetField("additionalPhonenumbers");
            field.SetValue(o, value);
        }
        
        private static void UpdateJa(object o, string field_name, string value)
        {
            FieldInfo field = typeof(JaStruct).GetField(field_name);
            field.SetValue(o, value);
        }

        private static PhoneNumberStruct CreatePhoneNumber(XmlNode node)
        {
            PhoneNumberStruct phonenumber = new PhoneNumberStruct();
            phonenumber.type = node.Attributes["type"].Value;
            phonenumber.phoneNumber = node.InnerText;

            return phonenumber;
        }

        public static JaStruct FetchPhone(string q, string access_code, string start= "1", string count="15", string cluster="true", string user_data="", string api_version = "2")
        {
            string url = "https://api.ja.is/phonebook/search?q=" + q + "&access_code=" + access_code + "&start=" + start + "&count=" + count + "&cluster=" + cluster + "&user_data=" + user_data + "&api_version=" + api_version;
            XmlDocument xmldoc = new XmlDocument();
            try
            {
                xmldoc.Load(url);
            }
            catch(System.Net.WebException) {
                JaStruct error_svar = new JaStruct();
                error_svar.error = "Unauthorized";
                return error_svar;
            }
            XmlNamespaceManager xnm1 = new XmlNamespaceManager(xmldoc.NameTable);
            XmlNodeList items = xmldoc.SelectNodes("result/items/item", xnm1);
            XmlNodeList meta = xmldoc.SelectNodes("result/meta", xnm1);


            object svar = RuntimeHelpers.GetObjectValue(new JaStruct());
            foreach (XmlNode metaitem in meta[0])
            {
                UpdateJa(svar, metaitem.Name, metaitem.InnerText);
            }

            List<ItemsStruct> itemstruct = new List<ItemsStruct>();

            foreach (XmlNode item in items)
            {
                object cur_item = RuntimeHelpers.GetObjectValue(new ItemsStruct());
                UpdateItem(cur_item, "type", item.Attributes["type"].Value);
                foreach (XmlNode Node in item)
                {
                    
                    if (!Node.Name.ToLower().Contains("phonenumber"))
                    {
                        UpdateItem(cur_item, Node.Name, Node.InnerText);
                    }
                    else
                    {
                     
                        if (Node.Name == "phoneNumber")
                        {
                            UpdatePhoneNumber(cur_item, CreatePhoneNumber(Node));
                        }
                        else if (Node.Name == "additionalPhoneNumbers")
                        {
                            List<PhoneNumberStruct> phoneNumbers = new List<PhoneNumberStruct>();
                            foreach (XmlNode addp in Node)
                            {
                                phoneNumbers.Add(CreatePhoneNumber(addp));
                            }
                            UpdateAddPhoneNumber(cur_item, phoneNumbers);
                        }
                    }
                }
                itemstruct.Add(((ItemsStruct)cur_item));
            }
            JaStruct svarid = (JaStruct)svar;
            svarid.items = itemstruct;
            return svarid;
        }

    }
}
