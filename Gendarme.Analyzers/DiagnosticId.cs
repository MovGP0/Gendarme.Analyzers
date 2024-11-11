namespace Gendarme.Analyzers;

public static class DiagnosticId
{
    #region Bad Practice
    public const string AssemblyVersionMismatch = "GENBP01";
    public const string AvoidCallingProblematicMethods = "GENBP02";
    public const string AvoidVisibleConstantField = "GENBP03";
    public const string CheckNewExceptionWithoutThrowing = "GENBP04";
    public const string CheckNewThreadWithoutStart = "GENBP05";
    public const string CloneMethodShouldNotReturnNull = "GENBP06";
    public const string ConstructorShouldNotCallVirtualMethods = "GENBP07";
    public const string DisableDebuggingCode = "GENBP08";
    public const string DoNotForgetNotImplementedMethods = "GENBP09";
    public const string DoNotUseEnumIsAssignableFrom = "GENBP0A";
    public const string DoNotUseGetInterfaceToCheckAssignability = "GENBP0B";
    public const string EqualsShouldHandleNullArg = "GENBP0C";
    public const string GetEntryAssemblyMayReturnNull = "GENBP0D";
    public const string ObsoleteMessagesShouldNotBeEmpty = "GENBP0E";
    public const string OnlyUseDisposeForIDisposableTypes = "GENBP0F";
    public const string PreferEmptyInstanceOverNull = "GENBP10";
    public const string PreferSafeHandle = "GENBP11";
    public const string ReplaceIncompleteOddnessCheck = "GENBP12";
    public const string ToStringShouldNotReturnNull = "GENBP13";
    #endregion

    #region Concurrency
    public const string DoNotLockOnThisOrTypes = "GENCN01";
    public const string DoNotLockOnWeakIdentityObjects = "GENCN02";
    public const string DoNotUseLockedRegionOutsideMethod = "GENCN03";
    public const string DoNotUseMethodImplOptionsSynchronized = "GENCN04";
    public const string DoNotUseThreadStaticWithInstanceFields = "GENCN05";
    public const string DoubleCheckLocking = "GENCN06";
    public const string NonConstantStaticFieldsShouldNotBeVisible = "GENCN07";
    public const string ProtectCallToEventDelegates = "GENCN08";
    public const string ReviewLockUsedOnlyForOperationsOnVariables = "GENCN09";
    public const string WriteStaticFieldFromInstanceMethod = "GENCN0A";
    #endregion

    #region Correctness
    public const string AttributeStringLiteralsShouldParseCorrectly = "GENCO01";
    public const string AvoidConstructorsInStaticTypes = "GENCO02";
    public const string AvoidFloatingPointEquality = "GENCO03";
    public const string BadRecursiveInvocation = "GENCO04";
    public const string CallingEqualsWithNullArg = "GENCO05";
    public const string CheckParametersNullityInVisibleMethods = "GENCO06";
    public const string DisposableFieldsShouldBeDisposed = "GENCO07";
    public const string DoNotCompareWithNaN = "GENCO08";
    public const string DoNotRecurseInEquality = "GENCO09";
    public const string DoNotRoundIntegers = "GENCO0A";
    public const string EnsureLocalDisposal = "GENCO0B";
    public const string FinalizersShouldCallBaseClassFinalizer = "GENCO0C";
    public const string MethodCanBeMadeStatic = "GENCO0D";
    public const string ProvideCorrectArgumentsToFormattingMethods = "GENCO0E";
    public const string ProvideCorrectRegexPattern = "GENCO0F";
    public const string ProvideValidXmlString = "GENCO10";
    public const string ProvideValidXPathExpression = "GENCO11";
    public const string ReviewCastOnIntegerDivision = "GENCO12";
    public const string ReviewCastOnIntegerMultiplication = "GENCO13";
    public const string ReviewDoubleAssignment = "GENCO14";
    public const string ReviewInconsistentIdentity = "GENCO15";
    public const string ReviewSelfAssignment = "GENCO16";
    public const string ReviewUselessControlFlow = "GENCO17";
    public const string ReviewUseOfInt64BitsToDouble = "GENCO18";
    public const string ReviewUseOfModuloOneOnIntegers = "GENCO19";
    public const string UseValueInPropertySetter = "GENCO1A";
    #endregion

    #region Design
    public const string AbstractTypesShouldNotHavePublicConstructors = "GENDE01";
    public const string AttributeArgumentsShouldHaveAccessors = "GENDE02";
    public const string AvoidEmptyInterface = "GENDE03";
    public const string AvoidMultidimensionalIndexer = "GENDE04";
    public const string AvoidPropertiesWithoutGetAccessor = "GENDE05";
    public const string AvoidRefAndOutParameters = "GENDE06";
    public const string AvoidSmallNamespace = "GENDE07";
    public const string AvoidVisibleFields = "GENDE08";
    public const string AvoidVisibleNestedTypes = "GENDE09";
    public const string ConsiderAddingInterface = "GENDE0A";
    public const string ConsiderConvertingFieldToNullable = "GENDE0B";
    public const string ConsiderConvertingMethodToProperty = "GENDE0C";
    public const string ConsiderUsingStaticType = "GENDE0D";
    public const string DeclareEventHandlersCorrectly = "GENDE0E";
    public const string DisposableTypesShouldHaveFinalizer = "GENDE0F";
    public const string DoNotDeclareProtectedMembersInSealedType = "GENDE10";
    public const string DoNotDeclareVirtualMethodsInSealedType = "GENDE11";
    public const string EnsureSymmetryForOverloadedOperators = "GENDE12";
    public const string EnumsShouldDefineAZeroValue = "GENDE13";
    public const string EnumsShouldUseInt32 = "GENDE14";
    public const string FinalizersShouldBeProtected = "GENDE15";
    public const string FlagsShouldNotDefineAZeroValue = "GENDE16";
    public const string ImplementEqualsAndGetHashCodeInPair = "GENDE17";
    public const string ImplementICloneableCorrectly = "GENDE18";
    public const string ImplementIComparableCorrectly = "GENDE19";
    public const string InternalNamespacesShouldNotExposeTypes = "GENDE1A";
    public const string MainShouldNotBePublic = "GENDE1B";
    public const string MarkAssemblyWithAssemblyVersion = "GENDE1C";
    public const string MarkAssemblyWithClsCompliant = "GENDE1D";
    public const string MarkAssemblyWithComVisible = "GENDE1E";
    public const string MissingAttributeUsageOnCustomAttribute = "GENDE1F";
    public const string OperatorEqualsShouldBeOverloaded = "GENDE20";
    public const string OverrideEqualsMethod = "GENDE21";
    public const string PreferEventsOverMethods = "GENDE22";
    public const string PreferIntegerOrStringForIndexers = "GENDE23";
    public const string PreferXmlAbstractions = "GENDE24";
    public const string ProvideAlternativeNamesForOperatorOverloads = "GENDE25";
    public const string TypesShouldBeInsideNamespaces = "GENDE26";
    public const string TypesWithDisposableFieldsShouldBeDisposable = "GENDE27";
    public const string TypesWithNativeFieldsShouldBeDisposable = "GENDE28";
    public const string UseCorrectDisposeSignatures = "GENDE29";
    public const string UseFlagsAttribute = "GENDE2A";
    #endregion
    
    #region Design.Generic
    public const string AvoidMethodWithUnusedGenericType = "GENDG01";
    public const string DoNotExposeNestedGenericSignatures = "GENDG02";
    public const string ImplementGenericCollectionInterfaces = "GENDG03";
    public const string PreferGenericsOverRefObject = "GENDG04";
    public const string UseGenericEventHandler = "GENDG05";
    #endregion

    #region Design.Linq
    public const string AvoidExtensionMethodOnSystemObject = "GENDL01";
    #endregion
    
    #region Exceptions
    public const string AvoidArgumentExceptionDefaultConstructor = "GENEX01";
    public const string AvoidThrowingBasicExceptions = "GENEX02";
    public const string DoNotDestroyStackTrace = "GENEX03";
    public const string DoNotSwallowErrorsCatchingNonSpecificExceptions = "GENEX04";
    public const string DoNotThrowInUnexpectedLocation = "GENEX05";
    public const string DoNotThrowReservedException = "GENEX06";
    public const string ExceptionShouldBeVisible = "GENEX07";
    public const string InstantiateArgumentExceptionCorrectly = "GENEX08";
    public const string MissingExceptionConstructors = "GENEX09";
    public const string UseObjectDisposedException = "GENEX0A";
    #endregion

    #region Interoperability
    public const string DelegatesPassedToNativeCodeMustIncludeExceptionHandling = "GENIN01";
    public const string DoNotAssumeIntPtrSize = "GENIN02";
    public const string GetLastErrorMustBeCalledRightAfterPInvoke = "GENIN03";
    public const string MarshalBooleansInPInvokeDeclarations = "GENIN04";
    public const string MarshalStringsInPInvokeDeclarations = "GENIN05";
    public const string PInvokeShouldNotBeVisible = "GENIN06";
    public const string UseManagedAlternativesToPInvoke = "GENIN07";
    #endregion
    
    #region Maintainability
    public const string AvoidAlwaysNullField = "GENMA01";
    public const string AvoidComplexMethods = "GENMA02";
    public const string AvoidDeepInheritanceTree = "GENMA03";
    public const string AvoidLackOfCohesionOfMethods = "GENMA04";
    public const string AvoidUnnecessarySpecialization = "GENMA05";
    public const string ConsiderUsingStopwatch = "GENMA06";
    public const string PreferStringIsNullOrEmpty = "GENMA07";
    #endregion

    #region Naming
    public const string AvoidDeepNamespaceHierarchy = "GENNA01";
    public const string AvoidNonAlphanumericIdentifier = "GENNA02";
    public const string AvoidRedundancyInMethodName = "GENNA03";
    public const string AvoidRedundancyInTypeName = "GENNA04";
    public const string AvoidTypeInterfaceInconsistency = "GENNA05";
    public const string DoNotPrefixEventsWithAfterOrBefore = "GENNA06";
    public const string DoNotPrefixValuesWithEnumName = "GENNA07";
    public const string DoNotUseReservedInEnumValueNames = "GENNA08";
    public const string ParameterNamesShouldMatchOverriddenMethod = "GENNA09";
    public const string UseCorrectCasing = "GENNA0A";
    public const string UseCorrectPrefix = "GENNA0B";
    public const string UseCorrectSuffix = "GENNA0C";
    public const string UsePluralNameInEnumFlags = "GENNA0D";
    public const string UsePreferredTerms = "GENNA0E";
    public const string UseSingularNameInEnumsUnlessAreFlags = "GENNA0F";
    #endregion
    
    #region Performance
    public const string AvoidLargeNumberOfLocalVariables = "GENPE01";
    public const string AvoidLargeStructure = "GENPE02";
    public const string AvoidRepetitiveCasts = "GENPE03";
    public const string AvoidReturningArraysOnProperties = "GENPE004";
    public const string AvoidTypeGetTypeForConstantStrings = "GENPE05";
    public const string AvoidUncalledPrivateCode = "GENPE06";
    public const string AvoidUninstantiatedInternalClasses = "GENPE07";
    public const string AvoidUnneededCallsOnString = "GENPE08";
    public const string AvoidUnneededFieldInitialization = "GENPE09";
    public const string AvoidUnneededUnboxing = "GENPE0A";
    public const string AvoidUnsealedConcreteAttributes = "GENPE0B";
    public const string AvoidUnsealedUninheritedInternalType = "GENPE0C";
    public const string AvoidUnusedParameters = "GENPE0D";
    public const string AvoidUnusedPrivateFields = "GENPE0E";
    public const string CompareWithEmptyStringEfficiently = "GENPE0F";
    public const string ConsiderCustomAccessorsForNonVisibleEvents = "GENPE10";
    public const string DoNotIgnoreMethodResult = "GENPE11";
    public const string ImplementEqualsType = "GENPE12";
    public const string MathMinMaxCandidate = "GENPE13";
    public const string OverrideValueTypeDefaults = "GENPE14";
    public const string PreferCharOverload = "GENPE15";
    public const string PreferLiteralOverInitOnlyFields = "GENPE16";
    public const string RemoveUnneededFinalizer = "GENPE17";
    public const string RemoveUnusedLocalVariables = "GENPE18";
    public const string ReviewLinqMethod = "GENPE19";
    public const string UseIsOperator = "GENPE1A";
    public const string UseStringEmpty = "GENPE1B";
    public const string UseSuppressFinalizeOnIDisposableTypeWithFinalizer = "GENPE1C";
    public const string UseTypeEmptyTypes = "GENPE1D";
    #endregion

    #region Portability
    public const string DoNotHardcodePaths = "GENPO01";
    public const string ExitCodeIsLimitedOnUnix = "GENPO02";
    public const string FeatureRequiresRootPrivilegeOnUnix = "GENPO03";
    public const string MonoCompatibilityReview = "GENPO04";
    public const string NewLineLiteral = "GENPO05";
    #endregion
    
    #region Security
    public const string ArrayFieldsShouldNotBeReadOnly = "GENSE01";
    public const string DoNotShortCircuitCertificateCheck = "GENSE02";
    public const string NativeFieldsShouldNotBeVisible = "GENSE03";
    public const string StaticConstructorsShouldBePrivate = "GENSE04";
    #endregion

    #region Security.CodeAccessSecurity
    public const string AddMissingTypeInheritanceDemand = "GENSC01";
    public const string DoNotExposeFieldsInSecuredType = "GENSC02";
    public const string DoNotExposeMethodsProtectedByLinkDemand = "GENSC03";
    public const string DoNotReduceTypeSecurityOnMethods = "GENSC04";
    public const string ReviewSealedTypeWithInheritanceDemand = "GENSC05";
    public const string ReviewSuppressUnmanagedCodeSecurityUsage = "GENSC06";
    public const string SecureGetObjectDataOverrides = "GENSC07";
    #endregion

    #region Serialization
    public const string CallBaseMethodsOnISerializableTypes = "GENSR01";
    public const string DeserializeOptionalField = "GENSR02";
    public const string ImplementISerializableCorrectly = "GENSR03";
    public const string MarkAllNonSerializableFields = "GENSR04";
    public const string MarkEnumerationsAsSerializable = "GENSR05";
    public const string MissingSerializableAttributeOnISerializableType = "GENSR06";
    public const string MissingSerializationConstructor = "GENSR07";
    public const string UseCorrectSignatureForSerializationMethods = "GENSR08";
    #endregion

    #region Smells
    public const string AvoidCodeDuplicatedInSameClass = "GENSM01";
    public const string AvoidCodeDuplicatedInSiblingClasses = "GENSM02";
    public const string AvoidLargeClasses = "GENSM03";
    public const string AvoidLongMethods = "GENSM04";
    public const string AvoidLongParameterLists = "GENSM05";
    public const string AvoidMessageChains = "GENSM06";
    public const string AvoidSpeculativeGenerality = "GENSM07";
    public const string AvoidSwitchStatements = "GENSM08";
    #endregion

    #region UI
    public const string GtkSharpExecutableTarget = "GENUI01";
    public const string SystemWindowsFormsExecutableTarget = "GENUI02";
    public const string UseStaThreadAttributeOnSwfEntryPoints = "GENUI03";
    #endregion
}
