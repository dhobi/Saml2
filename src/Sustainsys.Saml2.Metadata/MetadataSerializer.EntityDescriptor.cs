﻿using Sustainsys.Saml2.Metadata.Elements;
using Sustainsys.Saml2.Metadata.Xml;
using System.Xml;

namespace Sustainsys.Saml2.Metadata;

/// <summary>
/// Serializer for Saml2 Metadata
/// </summary>
public class MetadataSerializer
{
    /// <summary>
    /// Create EntityDescriptor instance. Override to use subclass.
    /// </summary>
    /// <returns>EntityDescriptor</returns>
    protected virtual EntityDescriptor CreateEntityDescriptor() => new();

    /// <summary>
    /// Read an EntityDescriptor
    /// </summary>
    /// <param name="xmlTraverser">Source data</param>
    /// <returns>EntityDescriptor</returns>
    public virtual EntityDescriptor ReadEntityDescriptor(XmlTraverser xmlTraverser)
    {
        xmlTraverser.EnsureName(Namespaces.Metadata, "EntityDescriptor");
        
        var entityDescriptor = CreateEntityDescriptor();
        
        ReadAttributes(xmlTraverser, entityDescriptor);

        xmlTraverser.ThrowOnErrors();

        return entityDescriptor;
    }

    /// <summary>
    /// Read attributes of EntityDescriptor
    /// </summary>
    /// <param name="xmlTraverser">Xml data to read</param>
    /// <param name="entityDescriptor">EntityDescriptor</param>
    protected virtual void ReadAttributes(XmlTraverser xmlTraverser, EntityDescriptor entityDescriptor)
    {
        entityDescriptor.EntityId = xmlTraverser.GetRequiredAbsoluteUriAttribute("entityID");
        entityDescriptor.Id = xmlTraverser.GetAttribute("ID");
        entityDescriptor.CacheDuraton = xmlTraverser.GetTimeSpanAttribute("cacheDuration");
        entityDescriptor.ValidUntil = xmlTraverser.GetDateTimeAttribute("validUntil");
    }
}
