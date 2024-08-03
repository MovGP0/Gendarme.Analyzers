namespace Gendarme.Analyzers;

public static class DiagnosticId
{
    #region Bad Practice

    public const string AssemblyVersionMismatch = "GEN0001";
    public const string AvoidCallingProblematicMethods = "GEN0002";
    public const string AvoidVisibleConstantField = "GEN0003";
    public const string CheckNewExceptionWithoutThrowing = "GEN0004";
    public const string CheckNewThreadWithoutStart = "GEN0005";
    public const string CloneMethodShouldNotReturnNull = "GEN0006";
    public const string ConstructorShouldNotCallVirtualMethods = "GEN0007";
    public const string DisableDebuggingCode = "GEN0008";
    public const string DoNotForgetNotImplementedMethods = "GEN0009";
    public const string DoNotUseEnumIsAssignableFrom = "GEN000A";
    public const string DoNotUseGetInterfaceToCheckAssignability = "GEN000B";
    public const string EqualsShouldHandleNullArg = "GEN000C";
    public const string GetEntryAssemblyMayReturnNull = "GEN000D";
    public const string ObsoleteMessagesShouldNotBeEmpty = "GEN000E";
    public const string OnlyUseDisposeForIDisposableTypes = "GEN000F";
    public const string PreferEmptyInstanceOverNull = "GEN0010";
    public const string PreferSafeHandle = "GEN0011";
    public const string ReplaceIncompleteOddnessCheck = "GEN0012";
    public const string ToStringShouldNotReturnNull = "GEN0013";

    #endregion

    #region Concurrency

    public const string DoNotLockOnThisOrTypes = "GEN0101";                    
    public const string DoNotLockOnWeakIdentityObjects = "GEN0102";            
    public const string DoNotUseLockedRegionOutsideMethod = "GEN0103";         
    public const string DoNotUseMethodImplOptionsSynchronized = "GEN0104";     
    public const string DoNotUseThreadStaticWithInstanceFields = "GEN0105";    
    public const string DoubleCheckLocking = "GEN0106";                        
    public const string NonConstantStaticFieldsShouldNotBeVisible = "GEN0107"; 
    public const string ProtectCallToEventDelegates = "GEN0108";               
    public const string ReviewLockUsedOnlyForOperationsOnVariables = "GEN0109";
    public const string WriteStaticFieldFromInstanceMethod = "GEN010A";

    #endregion
}