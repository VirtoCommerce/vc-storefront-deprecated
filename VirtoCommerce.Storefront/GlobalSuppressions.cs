
// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.

using System.CodeDom.Compiler;

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Sonar Vulnerability", "S2068:Credentials should not be hard-coded", Justification = "False-positive. This constant is the name of password cookie", Scope = "member", Target = "~F:VirtoCommerce.Storefront.Common.StorefrontConstants.PasswordResetTokenCookie")]
