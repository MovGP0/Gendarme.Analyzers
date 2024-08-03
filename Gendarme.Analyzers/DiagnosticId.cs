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
    public const string AvoidDeepNamespaceHierarchyAnalyzer = "GENNA01";
    public const string AvoidNonAlphanumericIdentifierAnalyzer = "GENNA02";
    public const string AvoidRedundancyInMethodNameAnalyzer = "GENNA03";
    public const string AvoidRedundancyInTypeNameAnalyzer = "GENNA04";
    public const string AvoidTypeInterfaceInconsistencyAnalyzer = "GENNA05";
    public const string DoNotPrefixEventsWithAfterOrBeforeAnalyzer = "GENNA06";
    public const string DoNotPrefixValuesWithEnumNameAnalyzer = "GENNA07";
    public const string DoNotUseReservedInEnumValueNamesAnalyzer = "GENNA08";
    public const string ParameterNamesShouldMatchOverriddenMethodAnalyzer = "GENNA09";
    public const string UseCorrectCasingAnalyzer = "GENNA0A";
    public const string UseCorrectPrefixAnalyzer = "GENNA0B";
    public const string UseCorrectSuffixAnalyzer = "GENNA0C";
    public const string UsePluralNameInEnumFlagsAnalyzer = "GENNA0D";
    public const string UsePreferredTermsAnalyzer = "GENNA0E";
    public const string UseSingularNameInEnumsUnlessAreFlagsAnalyzer = "GENNA0F";
    #endregion
    
    #region Performance
    public const string AvoidLargeNumberOfLocalVariablesAnalyzer = "GENPE01";
    public const string AvoidLargeStructureAnalyzer = "GENPE02";
    public const string AvoidRepetitiveCastsAnalyzer = "GENPE03";
    public const string AvoidReturningArraysOnPropertiesAnalyzer = "GENPE004";
    public const string AvoidTypeGetTypeForConstantStringsAnalyzer = "GENPE05";
    public const string AvoidUncalledPrivateCodeAnalyzer = "GENPE06";
    public const string AvoidUninstantiatedInternalClassesAnalyzer = "GENPE07";
    public const string AvoidUnneededCallsOnStringAnalyzer = "GENPE08";
    public const string AvoidUnneededFieldInitializationAnalyzer = "GENPE09";
    public const string AvoidUnneededUnboxingAnalyzer = "GENPE0A";
    public const string AvoidUnsealedConcreteAttributesAnalyzer = "GENPE0B";
    public const string AvoidUnsealedUninheritedInternalTypeAnalyzer = "GENPE0C";
    public const string AvoidUnusedParametersAnalyzer = "GENPE0D";
    public const string AvoidUnusedPrivateFieldsAnalyzer = "GENPE0E";
    public const string CompareWithEmptyStringEfficientlyAnalyzer = "GENPE0F";
    public const string ConsiderCustomAccessorsForNonVisibleEventsAnalyzer = "GENPE10";
    public const string DoNotIgnoreMethodResultAnalyzer = "GENPE11";
    public const string ImplementEqualsTypeAnalyzer = "GENPE12";
    public const string MathMinMaxCandidateAnalyzer = "GENPE13";
    public const string OverrideValueTypeDefaultsAnalyzer = "GENPE14";
    public const string PreferCharOverloadAnalyzer = "GENPE15";
    public const string PreferLiteralOverInitOnlyFieldsAnalyzer = "GENPE16";
    public const string RemoveUnneededFinalizerAnalyzer = "GENPE17";
    public const string RemoveUnusedLocalVariablesAnalyzer = "GENPE18";
    public const string ReviewLinqMethodAnalyzer = "GENPE19";
    public const string UseIsOperatorAnalyzer = "GENPE1A";
    public const string UseStringEmptyAnalyzer = "GENPE1B";
    public const string UseSuppressFinalizeOnIDisposableTypeWithFinalizerAnalyzer = "GENPE1C";
    public const string UseTypeEmptyTypesAnalyzer = "GENPE1D";
    #endregion

    #region Portability
    public const string DoNotHardcodePathsAnalyzer = "GENPO01";
    public const string ExitCodeIsLimitedOnUnixAnalyzer = "GENPO02";
    public const string FeatureRequiresRootPrivilegeOnUnixAnalyzer = "GENPO03";
    public const string MonoCompatibilityReviewAnalyzer = "GENPO04";
    public const string NewLineLiteralAnalyzer = "GENPO05";
    #endregion
    
    #region Security
    public const string ArrayFieldsShouldNotBeReadOnlyAnalyzer = "GENSE01";
    public const string DoNotShortCircuitCertificateCheckAnalyzer = "GENSE02";
    public const string NativeFieldsShouldNotBeVisibleAnalyzer = "GENSE03";
    public const string StaticConstructorsShouldBePrivateAnalyzer = "GENSE04";
    #endregion

    #region Security.CodeAccessSecurity
    public const string AddMissingTypeInheritanceDemandAnalyzer = "GENSC01";
    public const string DoNotExposeFieldsInSecuredTypeAnalyzer = "GENSC02";
    public const string DoNotExposeMethodsProtectedByLinkDemandAnalyzer = "GENSC03";
    public const string DoNotReduceTypeSecurityOnMethodsAnalyzer = "GENSC04";
    public const string ReviewSealedTypeWithInheritanceDemandAnalyzer = "GENSC05";
    public const string ReviewSuppressUnmanagedCodeSecurityUsageAnalyzer = "GENSC06";
    public const string SecureGetObjectDataOverridesAnalyzer = "GENSC07";
    #endregion

    #region Serialization
    public const string CallBaseMethodsOnISerializableTypesAnalyzer = "GENSR01";
    public const string DeserializeOptionalFieldAnalyzer = "GENSR02";
    public const string ImplementISerializableCorrectlyAnalyzer = "GENSR03";
    public const string MarkAllNonSerializableFieldsAnalyzer = "GENSR04";
    public const string MarkEnumerationsAsSerializableAnalyzer = "GENSR05";
    public const string MissingSerializableAttributeOnISerializableTypeAnalyzer = "GENSR06";
    public const string MissingSerializationConstructorAnalyzer = "GENSR07";
    public const string UseCorrectSignatureForSerializationMethodsAnalyzer = "GENSR08";
    #endregion

    #region Smells
    public const string AvoidCodeDuplicatedInSameClassAnalyzer = "GENSM01";
    public const string AvoidCodeDuplicatedInSiblingClassesAnalyzer = "GENSM02";
    public const string AvoidLargeClassesAnalyzer = "GENSM03";
    public const string AvoidLongMethodsAnalyzer = "GENSM04";
    public const string AvoidLongParameterListsAnalyzer = "GENSM05";
    public const string AvoidMessageChainsAnalyzer = "GENSM06";
    public const string AvoidSpeculativeGeneralityAnalyzer = "GENSM07";
    public const string AvoidSwitchStatementsAnalyzer = "GENSM08";
    #endregion
}
