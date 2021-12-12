using BytecodeApi.Extensions;
using BytecodeApi.Threading;
using BytecodeApi.UI.Data;
using PEunion.Compiler.Errors;
using PEunion.Compiler.Project;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace PEunion
{
	public sealed class ErrorModel : ObservableObject
	{
		private readonly Deferrer UpdateDeferrer;
		private bool UpdateDeferrerSkipNext;

		private ProjectModel _Project;
		private ObservableCollection<Error> _Errors = new ObservableCollection<Error>();
		private ObservableCollection<Error> _FilteredErrors = new ObservableCollection<Error>();
		private bool _ShowSeverityError = true;
		private bool _ShowSeverityWarning = true;
		private bool _ShowSeverityMessage = true;
		public ProjectModel Project
		{
			get => _Project;
			set => Set(ref _Project, value);
		}
		public ObservableCollection<Error> Errors
		{
			get => _Errors;
			set
			{
				if (Errors != null) Errors.CollectionChanged -= Errors_CollectionChanged;
				Set(ref _Errors, value);
				FilterErrors();
				if (Errors != null) Errors.CollectionChanged += Errors_CollectionChanged;
			}
		}
		public ObservableCollection<Error> FilteredErrors
		{
			get => _FilteredErrors;
			set => Set(ref _FilteredErrors, value);
		}
		public bool ShowSeverityError
		{
			get => _ShowSeverityError;
			set
			{
				Set(ref _ShowSeverityError, value);
				FilterErrors();
			}
		}
		public bool ShowSeverityWarning
		{
			get => _ShowSeverityWarning;
			set
			{
				Set(ref _ShowSeverityWarning, value);
				FilterErrors();
			}
		}
		public bool ShowSeverityMessage
		{
			get => _ShowSeverityMessage;
			set
			{
				Set(ref _ShowSeverityMessage, value);
				FilterErrors();
			}
		}
		public int SeverityCountError => Errors.Count(error => error.Severity == ErrorSeverity.Error);
		public int SeverityCountWarning => Errors.Count(error => error.Severity == ErrorSeverity.Warning);
		public int SeverityCountMessage => Errors.Count(error => error.Severity == ErrorSeverity.Message);

		public ErrorModel(ProjectModel project)
		{
			Project = project;
			UpdateDeferrer = new Deferrer(500, UpdateDeferrer_Invoke);
		}

		public void Validate(bool deferred)
		{
			UpdateDeferrer.InvokeDefault(!deferred);
		}
		public void SetErrors(IEnumerable<Error> errors)
		{
			Errors = errors.ToObservableCollection();
			UpdateDeferrerSkipNext = true;
			UpdateDeferrer.InvokeDefault(true);
		}
		public ErrorCollection GetProjectErrors()
		{
			ErrorCollection errors = new ErrorCollection();
			ProjectFile.FromString(Project.ProjectPath, ProjectConverter.ToProjectFile(Project).AsString(), errors);
			return errors;
		}
		private void FilterErrors()
		{
			FilteredErrors = Errors
				.Where(error => ShowSeverityError || error.Severity != ErrorSeverity.Error)
				.Where(error => ShowSeverityWarning || error.Severity != ErrorSeverity.Warning)
				.Where(error => ShowSeverityMessage || error.Severity != ErrorSeverity.Message)
				.OrderByDescending(error => error.Severity)
				.ToObservableCollection();

			RaisePropertyChanged(nameof(SeverityCountError));
			RaisePropertyChanged(nameof(SeverityCountWarning));
			RaisePropertyChanged(nameof(SeverityCountMessage));
		}

		private void UpdateDeferrer_Invoke()
		{
			if (UpdateDeferrerSkipNext)
			{
				UpdateDeferrerSkipNext = false;
			}
			else
			{
				Errors = GetProjectErrors().ToObservableCollection();
			}
		}
		private void Errors_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			FilterErrors();
		}
	}
}