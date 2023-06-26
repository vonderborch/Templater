# Templater
Wanting to make a template from a project but frustrated at Visual Studio for making creating templates from complex solutions complicated (especially once you start trying to add custom logic to things...)? So was I. This isn't a perfect solution, but it at least makes things easier!

This is my [second attempt](https://github.com/vonderborch/SolutionCreator) at this and has been rewritten from the ground up to have much less sphagetti code everywhere.

# Download
The latest release is available on the releases page: https://github.com/vonderborch/Templater/releases/latest

# Execution

## App
Coming soon!

## Command Prompt/Terminal Version
- Creating a new Solution: `./Templater.exe generate`
    - Available Parameters: `./Templater.exe help generate`
- Create a new Template: `./Templater.exe prepare`
    - Available Parameters: `./Templater.exe help prepare`
- List Available Templates: `./Templater.exe list-templates`
    - Available Parameters: `./Templater.exe help -list-templates`
- Update or Download New Templates: `./Templater.exe update-templates`
    - Available Parameters: `./Templater.exe help update-templates`
- Update/set Git Settings: `./Templater.exe configure`
    - Available Parameters: `./Templater.exe help configure`
    - NOTE: Right now the code saves this data in a plaintext. This is not good, but at some point I hope to rework it to be better.
- Check for App Updates: `./Templater.exe update`
    - Available Parameters: `./Templater.exe help update`
- Report an Issue: `./Templater.exe report-issue`
    - Available Parameters: `./Templater.exe help report-issue`
- Get Help: `./Templater.exe help`
- Display Current Version: `./Templeter.exe version`

# Existing Templates
The application will automatically download templates from the [Template Repository](https://github.com/vonderborch/Templater-Templates). Currently available templates are:
- Velentr.BASE: A simple library that isn't tied to anything XNA related
- Velentr.BASE_DUAL_SUPPORT: A library that has two different implementations: one for FNA and one for Monogame
- Velentr.GENERIC_DUAL_SUPPORT: A library that has one generic implementation (not tied to FNA or Monogame) and then either extensions or custom implementations for FNA and Monogame

# Creating a New Template
Create a solution, then run `./Templater.exe prepare` against the solution!

# Known Issues
- Brittle, not a lot of validation checking means it can crash easily and won't tell you what went wrong too well...

Have an issue? https://github.com/vonderborch/Templater/issues

# Future Plans
See list of issues under the Milestones: https://github.com/vonderborch/Templater/milestones
