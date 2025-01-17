﻿using System;
using MonoDevelop.Projects;
using System.Xml;
using MonoDevelop.Core.Assemblies;

namespace MonoDevelop.MonoGame
{
	public static class MonoGameBuildAction
	{
		public static readonly string Shader;

		public static bool IsMonoGameBuildAction(string action){
			return action == Shader;
		}
		static MonoGameBuildAction(){
			Shader = "MonoGameShader";
		}
	}

	public static class MonoGameProject : DotNetAssemblyProject
	{
		public MonoGameProject ()
		{
			Init ();
		}
		public MonoGameProject (string languageName)
			: base (languageName)
		{
			Init ();
		}
		public MonoGameProject(string languageName, ProjectCreateInformation info, XmlElement projectOptions)
			:base(languageName, info, projectOptions)
		{
			Init ();
		}
		private void Init(){
		
		}

		public override SolutionItemConfiguration CreateConfiguration(string name)
		{
			var conf = new MonoGameProjectConfiguration (name);
			conf.CopyFrom (base.CreateConfiguration (name));
			return conf;

		}
		public override bool SupportsFormat (FileFormat format)
		{
			return format.Id == "MSBuild10";
		}
		public override TargetFrameworkMoniker GetDefaultTargetFrameworkForFormat (FileFormat format)
		{
			return new TargetFrameworkMoniker("4.0");
		}
		public override bool SupportsFramework (MonoDevelop.Core.Assemblies.TargetFramework framework)
		{

			if (framework.Id != (MonoDevelop.Core.Assemblies.TargetFrameworkMoniker.NET_4_0))
				return false;
			else
			return base.SupportsFramework (framework);
		}
		protected override System.Collections.Generic.IList<string> GetCommonBuildActions ()
		{
			var actions = new System.Collections.Generic.List<string> (base.GetCommonBuildActions ());
			actions.Add (MonoGameBuildAction.Shader);

			return actions;
		}
		public override string GetDefaultBuildAction (string fileName)
		{
			if (System.IO.Path.GetExtension (fileName) == ".fx") {
				return MonoGameBuildAction.Shader;
			}
			return base.GetDefaultBuildAction (fileName);
		}
		protected override void PopulateSupportFileList (FileCopySet list, ConfigurationSelector configuration)
		{
			base.PopulateSupportFileList (list, configuration);
			foreach(var projectReference in References){
				if (projectReference != null && projectReference.Package.Name == "monogame") {
					if (projectReference.ReferenceType == ReferenceType.Gac) {
						foreach (var assem in projectReference.Package.Assemblies) {
							list.Add (assem.Location);
							var cfg = (MonoGameProjectConfiguration)configuration.GetConfiguration (this);
							if (cfg.DebugMode) {
								var mdbFile = TargetRuntime.GetAssemblyDebugInfoFile (assem.Location);
								if (System.IO.File.Equals(mdbFile)){
									list.Add(mdbFile);
								}
							}
						}
					}
					break;
				}

			}
		}
	}

	public class MonoGameBuildExtension : ProjectServiceExtension{
		protected override BuildResult Build (MonoDevelop.Core.IProgressMonitor monitor, IBuildTarget item, ConfigurationSelector configuration)
		{
			try {
				return base.Build(monitor, item, configuration);
			} finally {
	
			}
		}
		protected override BuildResult Compile (MonoDevelop.Core.IProgressMonitor monitor, SolutionEntityItem item, BuildData buildData)
		{
			try {
				var proj = item as MonoGameProject;
				if (proj == null){
					return base.Compile (monitor, item, buildData);
				}
				var results = new System.Collections.Generic.List<BuildResult>();
				foreach (var file in proj.Files) {
					if (MonoGameBuildAction.IsMonoGameBuildAction(file.BuildAction)){
						buildData.Items.Add(file);
						var buildresults = MonoGameContentProcessor.Compile(file, monitor,buildData);
						results.Add(buildresults);
					}
				
				}
				return base.Compile(monitor, item, buildData).Append(results);
			} finally {
	
			}

		}
	}
	public class MonoGameProjectBinding : IProjectBinding{
		public Project CreateProject(ProjectCreateInformation info, System.Xml.XmlElement projectOptions){
			string lang = projectOptions.GetAttribute ("language");
			return new MonoGameProject (lang, info, projectOptions);
		}

		public Project CreateSingleFileproject(string sourceFile){
			throw new InvalidOperationException ();
		}
		public Project CanCreateSingleFileProject(string sourceFile){
			return false;
		}
		public string Name{
			get{return "MonoGame"; }
		}
	}

	public class MonoGameProjectConfiguration : DotNetProjectConfiguration{

		public MonoGameProjectConfiguration () : base (){
		}
	
		public MonoGameProjectConfiguration (string name): base(name){
		}

		public override void CopyFrom (ItemConfiguration configuration)
		{
			base.CopyFrom (configuration);
		}
	}
	public class MonoGameContentProcessor {
		public static BuildResult Compile( ProjectFile file, MonoDevelop.Core.IProgressMonitor monitor, BuildData buildData){
		
			switch (file.BuildAction) {
			case "MonoGameShader":
				{
					var results = new BuildResult ();
					monitor.Log.WriteLine ("Compiling Shader");
					monitor.Log.WriteLine ("Shader : " + buildData.Configuration.OutputDirectory);
					monitor.Log.WriteLine ("Shader : " + file.FilePath);
					monitor.Log.WriteLine ("Shader : " + file.ToString ());
					return results;
				}
			default:
				return new BuildResult ();
			}
		}
	}
}

