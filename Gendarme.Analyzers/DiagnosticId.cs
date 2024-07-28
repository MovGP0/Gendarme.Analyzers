namespace Gendarme.Analyzers;

public static class DiagnosticId
{
    #region Bad Practice

    public static readonly string AssemblyVersionMismatch = "GEN0001";
    public static readonly string AvoidCallingProblematicMethods = "GEN0002";
    public static readonly string AvoidVisibleConstantField = "GEN0003";
    public static readonly string CheckNewExceptionWithoutThrowing = "GEN0004";
    public static readonly string CheckNewThreadWithoutStart = "GEN0005";
    public static readonly string CloneMethodShouldNotReturnNull = "GEN0006";
    public static readonly string ConstructorShouldNotCallVirtualMethods = "GEN0007";
    public static readonly string DisableDebuggingCode = "GEN0008";
    public static readonly string DoNotForgetNotImplementedMethods = "GEN0009";
    public static readonly string DoNotUseEnumIsAssignableFrom = "GEN000A";
    public static readonly string DoNotUseGetInterfaceToCheckAssignability = "GEN000B";
    public static readonly string EqualsShouldHandleNullArg = "GEN000C";
    public static readonly string GetEntryAssemblyMayReturnNull = "GEN000D";
    public static readonly string ObsoleteMessagesShouldNotBeEmpty = "GEN000E";
    public static readonly string OnlyUseDisposeForIDisposableTypes = "GEN000F";
    public static readonly string PreferEmptyInstanceOverNull = "GEN0010";
    public static readonly string PreferSafeHandle = "GEN0011";
    public static readonly string ReplaceIncompleteOddnessCheck = "GEN0012";
    public static readonly string ToStringShouldNotReturnNull = "GEN0013";

    #endregion
}