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
    
    #region Correctness

    public const string AttributeStringLiteralsShouldParseCorrectly = "GEN0201";
    public const string AvoidConstructorsInStaticTypes = "GEN0202";
    public const string AvoidFloatingPointEquality = "GEN0203";
    public const string BadRecursiveInvocation = "GEN0204";
    public const string CallingEqualsWithNullArg = "GEN0205";
    public const string CheckParametersNullityInVisibleMethods = "GEN0206";
    public const string DisposableFieldsShouldBeDisposed = "GEN0207";
    public const string DoNotCompareWithNaN = "GEN0208";
    public const string DoNotRecurseInEquality = "GEN0209";
    public const string DoNotRoundIntegers = "GEN020A";
    public const string EnsureLocalDisposal = "GEN020B";
    public const string FinalizersShouldCallBaseClassFinalizer = "GEN020C";
    public const string MethodCanBeMadeStatic = "GEN020D";
    public const string ProvideCorrectArgumentsToFormattingMethods = "GEN020E";
    public const string ProvideCorrectRegexPattern = "GEN020F";
    public const string ProvideValidXmlString = "GEN0210";
    public const string ProvideValidXPathExpression = "GEN0211";
    public const string ReviewCastOnIntegerDivision = "GEN0212";
    public const string ReviewCastOnIntegerMultiplication = "GEN0213";
    public const string ReviewDoubleAssignment = "GEN0214";
    public const string ReviewInconsistentIdentity = "GEN0215";
    public const string ReviewSelfAssignment = "GEN0216";
    public const string ReviewUselessControlFlow = "GEN0217";
    public const string ReviewUseOfInt64BitsToDouble = "GEN0218";
    public const string ReviewUseOfModuloOneOnIntegers = "GEN0219";
    public const string UseValueInPropertySetter = "GEN021A";

    #endregion

}