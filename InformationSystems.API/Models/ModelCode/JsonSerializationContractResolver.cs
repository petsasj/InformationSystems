﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using InformationSystems.API;
namespace InformationSystems.API
{
    public class TelecommunicationInfrastructureJsonSerializationContractResolver : Newtonsoft.Json.Serialization.DefaultContractResolver
    {
        public bool SerializeCollections { get; set; } = false;
        public bool SerializeReferences { get; set; } = true;
        public bool SerializeByteArrays { get; set; } = true;
        readonly XPDictionary dictionary;

        public TelecommunicationInfrastructureJsonSerializationContractResolver()
        {
            dictionary = new ReflectionDictionary();
            dictionary.GetDataStoreSchema(new Type[] {
                typeof(Company),
                typeof(InfrastructureModificationRequest),
                typeof(JobsLog),
                typeof(Infrastructure),
                typeof(GeoLocation)
            });
        }

        protected override List<MemberInfo> GetSerializableMembers(Type objectType)
        {
            XPClassInfo classInfo = dictionary.QueryClassInfo(objectType);
            if (classInfo != null && classInfo.IsPersistent)
            {
                var allSerializableMembers = base.GetSerializableMembers(objectType);
                var serializableMembers = new List<MemberInfo>(allSerializableMembers.Count);
                foreach (MemberInfo member in allSerializableMembers)
                {
                    XPMemberInfo mi = classInfo.FindMember(member.Name);
                    if (!(mi.IsPersistent || mi.IsAliased || mi.IsCollection || mi.IsManyToManyAlias)
                        || ((mi.IsCollection || mi.IsManyToManyAlias) && !SerializeCollections)
                        || (mi.ReferenceType != null && !SerializeReferences)
                        || (mi.MemberType == typeof(byte[]) && !SerializeByteArrays))
                    {
                        continue;
                    }
                    serializableMembers.Add(member);
                }
                return serializableMembers;
            }
            return base.GetSerializableMembers(objectType);
        }
    }
}
namespace Microsoft.Extensions.DependencyInjection
{
    public static class TelecommunicationInfrastructureJsonMvcBuilderExtensions
    {
        public static IMvcBuilder AddTelecommunicationInfrastructureJsonOptions(this IMvcBuilder builder, Action<TelecommunicationInfrastructureJsonSerializationContractResolver> setupAction = null)
        {
            return builder.AddNewtonsoftJson(opt =>
            {
                var resolver = new TelecommunicationInfrastructureJsonSerializationContractResolver();
                opt.SerializerSettings.ContractResolver = resolver;
                opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                setupAction?.Invoke(resolver);
            });
        }
    }
}
