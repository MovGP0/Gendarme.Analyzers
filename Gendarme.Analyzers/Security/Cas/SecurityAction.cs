namespace Gendarme.Analyzers.Security.Cas;

/// <summary>
/// Specifies the security actions that can be performed using declarative security.
/// https://learn.microsoft.com/en-us/dotnet/api/system.security.permissions.securityaction?view=netframework-4.8.1
/// </summary>
internal enum SecurityAction
{
    /// <summary>
    /// All callers higher in the call stack are required to have been granted the permission specified
    /// by the current permission object.
    /// </summary>
    Demand = 2,

    /// <summary>
    /// The calling code can access the resource identified by the current permission object,
    /// even if callers higher in the stack have not been granted permission to access the
    /// resource (see Using the Assert Method).
    /// </summary>
    Assert = 3,

    /// <summary>
    /// The ability to access the resource specified by the current permission object is denied to callers,
    /// even if they have been granted permission to access it (see Using the Deny Method).
    /// </summary>
    Deny  = 4,

    /// <summary>
    /// Only the resources specified by this permission object can be accessed,
    /// even if the code has been granted permission to access other resources.
    /// </summary>
    PermitOnly  = 5,

    /// <summary> 
    /// The immediate caller is required to have been granted the specified permission.Do not use in the .NET Framework 4.
    /// For full trust, use SecurityCriticalAttribute instead;
    /// for partial trust, use Demand.
    /// </summary>
    LinkDemand  = 6,

    /// <summary>
    /// The derived class inheriting the class or overriding a method is required to have been granted the specified permission.
    /// </summary>
    InheritanceDemand  = 7,

    /// <summary>
    /// The request for the minimum permissions required for code to run.
    /// This action can only be used within the scope of the assembly.
    /// </summary>
    RequestMinimum  = 8,

    /// <summary>
    /// The request for additional permissions that are optional (not required to run).
    /// This request implicitly refuses all other permissions not specifically requested.
    /// This action can only be used within the scope of the assembly.
    /// </summary>
    RequestOptional  = 9,

    /// <summary>
    /// The request that permissions that might be misused will not be granted to the calling code.This action can only be used within the scope of the assembly.
    /// </summary>
    RequestRefuse = 10
}