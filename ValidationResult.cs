using System.Collections.Generic;

public struct ValidationResult
{
	public List<string> validationErrors;
	public bool validationSucess;

	public ValidationResult(bool validationSucess, List<string> validationErrors)
	{
		this.validationErrors = validationErrors;
		this.validationSucess = validationSucess;
	}
}
