
Setup<ProjectProperties>(context =>
{
    var projectProps = LoadProjectProperties(MakeAbsolute(Directory(".")));
    return projectProps;
});
