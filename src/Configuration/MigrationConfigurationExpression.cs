using System;
using LinqToDB.MigrateUp.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LinqToDB.MigrateUp.Configuration;

/// <summary>
/// Represents a configuration expression used during the creation of migration configurations.
/// It helps to construct and organize multiple migration profiles.
/// </summary>
public class MigrationConfigurationExpression
{
    /// <summary>
    /// Gets the list of migration profiles defined in this configuration.
    /// </summary>
    public List<MigrationProfile> Profiles { get; }


    /// <summary>
    /// Initializes a new instance of the <see cref="MigrationConfigurationExpression"/> class.
    /// </summary>
    public MigrationConfigurationExpression()
    {
        Profiles = new List<MigrationProfile>();
    }


    /// <summary>
    /// Adds a migration profile of the specified type to the configuration.
    /// </summary>
    /// <param name="profileType">The type of the migration profile to be added. It must be a subclass of <see cref="MigrationProfile"/>.</param>
    public void AddProfile(Type profileType)
    {
        var profile = Activator.CreateInstance(profileType) as MigrationProfile;
        if (profile == null)
        {
            throw new ArgumentException("Profile type must be a subclass of MigrationProfile", profileType.FullName);
        }

        AddProfile(profile);
    }


    /// <summary>
    /// Adds a migration profile of the specified generic type to the configuration.
    /// </summary>
    /// <typeparam name="TProfile">The type of the migration profile to be added. It must be a subclass of <see cref="MigrationProfile"/> and have a parameterless constructor.</typeparam>
    public void AddProfile<TProfile>() where TProfile : MigrationProfile, new()
    {
        AddProfile(new TProfile());
    }


    /// <summary>
    /// Adds a migration profile instance to the configuration.
    /// </summary>
    /// <param name="profile">The instance of the migration profile to be added.</param>
    public void AddProfile(MigrationProfile profile)
    {
        Profiles.Add(profile);
    }


    /// <summary>
    /// Adds all migration profiles found in the specified assembly to the configuration.
    /// </summary>
    /// <param name="assembly">The assembly in which to search for migration profiles.</param>
    public void AddProfiles(Assembly assembly)
    {
        assembly.GetTypes()
            .Where(x => typeof(MigrationProfile).IsAssignableFrom(x) && !x.IsAbstract)
            .ToList()
            .ForEach(x => AddProfile(x));
    }
}
