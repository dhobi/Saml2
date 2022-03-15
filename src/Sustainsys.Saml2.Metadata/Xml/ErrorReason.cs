﻿namespace Sustainsys.Saml2.Metadata.Xml;

/// <summary>
/// Error reasons in the error reporting.
/// </summary>
public enum ErrorReason
{
    /// <summary>
    /// The local of the node name was not the expected.
    /// </summary>
    UnexpectedLocalName,

    /// <summary>
    /// The namesapce of the node was not the expected.
    /// </summary>
    UnexpectedNamespace,

    /// <summary>
    /// A required attribute was missing.
    /// </summary>
    MissingAttribute,
    
    /// <summary>
    /// Value conversion failed for the attribute. The string
    /// representation is stored as <see cref="Error.StringValue"/>.
    /// </summary>
    ConversionFailed,

    /// <summary>
    /// A string value that should be an absolute Uri wasn't that.
    /// </summary>
    NotAbsoluteUri,
}
