﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:2.1.0.0
//      SpecFlow Generator Version:2.0.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace TripToPrint.Core.Tests.SpecflowTests
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.1.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute()]
    public partial class GroupsGenerationFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "GroupsGeneration.feature"
#line hidden
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute()]
        public static void FeatureSetup(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext)
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner(null, 0);
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Groups Generation", "\tTest different scenarios of dividing placemarks by groups", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute()]
        public static void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute()]
        public virtual void TestInitialize()
        {
            if (((testRunner.FeatureContext != null) 
                        && (testRunner.FeatureContext.FeatureInfo.Title != "Groups Generation")))
            {
                TripToPrint.Core.Tests.SpecflowTests.GroupsGenerationFeature.FeatureSetup(null);
            }
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute()]
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Scenario for two groups: 4 + 3 placemarks")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Groups Generation")]
        public virtual void ScenarioForTwoGroups43Placemarks()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Scenario for two groups: 4 + 3 placemarks", ((string[])(null)));
#line 4
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "Longitude",
                        "Latitude",
                        "Name"});
            table1.AddRow(new string[] {
                        "28.0552",
                        "-16.73494",
                        "Restaurante La Gula @ Los Cristianos"});
            table1.AddRow(new string[] {
                        "28.05622",
                        "-16.72488",
                        "El Aderno @ Los Cristianos"});
            table1.AddRow(new string[] {
                        "28.05273",
                        "-16.71508",
                        "Panaria @ Los Cristianos"});
            table1.AddRow(new string[] {
                        "28.05145",
                        "-16.72196",
                        "El Pincho @ Los Cristianos"});
            table1.AddRow(new string[] {
                        "28.03397",
                        "-16.6412",
                        "La Dulce Emilia @ Guargacho"});
            table1.AddRow(new string[] {
                        "28.11924",
                        "-16.67092",
                        "Restaurante El Chamo @ La Escalona"});
            table1.AddRow(new string[] {
                        "28.12183",
                        "-16.7406",
                        "Tandem Paragliding @ Adeje"});
#line 5
 testRunner.Given("I have these placemarks in my folder:", ((string)(null)), table1, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "Group Index",
                        "Name"});
            table2.AddRow(new string[] {
                        "0",
                        "Restaurante La Gula @ Los Cristianos"});
            table2.AddRow(new string[] {
                        "0",
                        "El Aderno @ Los Cristianos"});
            table2.AddRow(new string[] {
                        "0",
                        "Panaria @ Los Cristianos"});
            table2.AddRow(new string[] {
                        "0",
                        "El Pincho @ Los Cristianos"});
            table2.AddRow(new string[] {
                        "1",
                        "La Dulce Emilia @ Guargacho"});
            table2.AddRow(new string[] {
                        "1",
                        "Restaurante El Chamo @ La Escalona"});
            table2.AddRow(new string[] {
                        "1",
                        "Tandem Paragliding @ Adeje"});
#line 14
 testRunner.Then("these placemarks will be assigned to the following groups:", ((string)(null)), table2, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
