using CombatNarratationHelper.Models;
using System.Text.RegularExpressions;

namespace CombatNarratationHelper.Services;

public class StatblockParserService
{
    public MonsterStatblock ParseStatblock(string statblockText)
    {
        if (string.IsNullOrWhiteSpace(statblockText))
            return new MonsterStatblock();

        var monster = new MonsterStatblock();
        var lines = statblockText.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                                .Select(line => line.Trim())
                                .Where(line => !string.IsNullOrEmpty(line))
                                .ToArray();

        if (lines.Length == 0)
            return monster;

        try
        {
            int lineIndex = 0;

            // Parse name (first line)
            if (lineIndex < lines.Length)
            {
                monster.Name = lines[lineIndex].Trim();
                lineIndex++;
            }

            // Parse size, type, alignment (second line)
            if (lineIndex < lines.Length)
            {
                ParseSizeTypeAlignment(lines[lineIndex], monster);
                lineIndex++;
            }

            // Section parsing state
            string? currentSection = null;
            List<string> sectionBuffer = new();

            // Parse the rest of the statblock
            while (lineIndex < lines.Length)
            {
                var line = lines[lineIndex];

                // Section headers
                if (line.Equals("Traits", StringComparison.OrdinalIgnoreCase) ||
                    line.Equals("Actions", StringComparison.OrdinalIgnoreCase) ||
                    line.Equals("Legendary Actions", StringComparison.OrdinalIgnoreCase))
                {
                    // Save previous section
                    if (currentSection != null && sectionBuffer.Count > 0)
                        SaveSection(currentSection, sectionBuffer, monster);
                    currentSection = line;
                    sectionBuffer.Clear();
                    lineIndex++;
                    continue;
                }

                // Sectional content
                if (currentSection != null)
                {
                    // End of section if next section header or end of file
                    if (IsSectionHeader(line))
                    {
                        SaveSection(currentSection, sectionBuffer, monster);
                        currentSection = null;
                        sectionBuffer.Clear();
                        continue;
                    }
                    sectionBuffer.Add(line);
                    lineIndex++;
                    continue;
                }

                // AC
                if (line.StartsWith("Armor Class", StringComparison.OrdinalIgnoreCase) || line.StartsWith("AC", StringComparison.OrdinalIgnoreCase))
                {
                    ParseArmorClass(line, monster);
                }
                // Initiative
                else if (line.StartsWith("Initiative", StringComparison.OrdinalIgnoreCase))
                {
                    ParseInitiative(line, monster);
                }
                // HP
                else if (line.StartsWith("Hit Points", StringComparison.OrdinalIgnoreCase) || line.StartsWith("HP", StringComparison.OrdinalIgnoreCase))
                {
                    ParseHitPoints(line, monster);
                }
                // Speed
                else if (line.StartsWith("Speed", StringComparison.OrdinalIgnoreCase))
                {
                    ParseSpeed(line, monster);
                }
                // Ability Table
                else if (line.StartsWith("Mod", StringComparison.OrdinalIgnoreCase) && line.Contains("Save"))
                {
                    // Parse ability table (next 6 lines)
                    for (int i = 1; i <= 6 && lineIndex + i < lines.Length; i++)
                    {
                        ParseAbilityTableLine(lines[lineIndex + i], monster);
                    }
                    lineIndex += 6; // skip the 6 ability lines
                }
                // Skills
                else if (line.StartsWith("Skills", StringComparison.OrdinalIgnoreCase))
                {
                    ParseSkills(line, monster);
                }
                // Immunities
                else if (line.StartsWith("Immunities", StringComparison.OrdinalIgnoreCase))
                {
                    ParseImmunities(line, monster);
                }
                // Senses
                else if (line.StartsWith("Senses", StringComparison.OrdinalIgnoreCase))
                {
                    ParseSenses(line, monster);
                }
                // Languages
                else if (line.StartsWith("Languages", StringComparison.OrdinalIgnoreCase))
                {
                    ParseLanguages(line, monster);
                }
                // CR and PB
                else if (line.StartsWith("CR", StringComparison.OrdinalIgnoreCase))
                {
                    ParseCRandPB(line, monster);
                }
                // Legendary Action Uses
                else if (line.StartsWith("Legendary Action Uses", StringComparison.OrdinalIgnoreCase))
                {
                    ParseLegendaryActionUses(line, monster);
                }
                lineIndex++;
            }
            // Save last section if any
            if (currentSection != null && sectionBuffer.Count > 0)
                SaveSection(currentSection, sectionBuffer, monster);
        }
        catch (Exception)
        {
            // If parsing fails, return what we have so far
        }

        return monster;
    }

    private bool IsSectionHeader(string line)
    {
        return line.Equals("Traits", StringComparison.OrdinalIgnoreCase) ||
               line.Equals("Actions", StringComparison.OrdinalIgnoreCase) ||
               line.Equals("Legendary Actions", StringComparison.OrdinalIgnoreCase);
    }

    private void SaveSection(string section, List<string> buffer, MonsterStatblock monster)
    {
        var features = ParseFeatures(buffer);
        if (section.Equals("Traits", StringComparison.OrdinalIgnoreCase))
            monster.Traits.AddRange(features);
        else if (section.Equals("Actions", StringComparison.OrdinalIgnoreCase))
            monster.Actions.AddRange(features.Select(f => new MonsterAction { Name = f.Name, Description = f.Description }));
        else if (section.Equals("Legendary Actions", StringComparison.OrdinalIgnoreCase))
            monster.LegendaryActions.AddRange(features.Select(f => new MonsterAction { Name = f.Name, Description = f.Description }));
    }

    private List<MonsterFeature> ParseFeatures(List<string> lines)
    {
        var features = new List<MonsterFeature>();
        string? currentName = null;
        List<string> descBuffer = new();
        foreach (var line in lines)
        {
            // Feature/action name ends with a period
            if (line.EndsWith("."))
            {
                if (currentName != null)
                {
                    features.Add(new MonsterFeature { Name = currentName, Description = string.Join(" ", descBuffer).Trim() });
                    descBuffer.Clear();
                }
                currentName = line.TrimEnd('.');
            }
            else
            {
                descBuffer.Add(line);
            }
        }
        if (currentName != null)
            features.Add(new MonsterFeature { Name = currentName, Description = string.Join(" ", descBuffer).Trim() });
        return features;
    }

    private void ParseSizeTypeAlignment(string line, MonsterStatblock monster)
    {
        var parts = line.Split(',');
        if (parts.Length >= 2)
        {
            var sizeType = parts[0].Trim();
            monster.Alignment = parts[1].Trim();
            var sizeTypeMatch = Regex.Match(sizeType, @"^(\w+)\s+(.+)$");
            if (sizeTypeMatch.Success)
            {
                monster.Size = sizeTypeMatch.Groups[1].Value;
                monster.Type = sizeTypeMatch.Groups[2].Value;
            }
        }
    }

    private void ParseArmorClass(string line, MonsterStatblock monster)
    {
        var match = Regex.Match(line, @"(Armor Class|AC)\s+(\d+)(?:\s*\(([^)]+)\))?", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            if (int.TryParse(match.Groups[2].Value, out int ac))
            {
                monster.ArmorClass = ac;
            }
            monster.ArmorClassDescription = match.Groups[3].Value;
        }
    }

    private void ParseInitiative(string line, MonsterStatblock monster)
    {
        // Example: Initiative +12 (22)
        var match = Regex.Match(line, @"Initiative\s*([+-]?\d+)(?:\s*\((\d+)\))?", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            if (int.TryParse(match.Groups[1].Value, out int init))
                monster.Initiative = init;
        }
    }

    private void ParseHitPoints(string line, MonsterStatblock monster)
    {
        var match = Regex.Match(line, @"(Hit Points|HP)\s+(\d+)(?:\s*\(([^)]+)\))?", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            if (int.TryParse(match.Groups[2].Value, out int hp))
            {
                monster.HitPoints = hp;
            }
            monster.HitDice = match.Groups[3].Value;
        }
    }

    private void ParseSpeed(string line, MonsterStatblock monster)
    {
        var speedText = line.Replace("Speed", "", StringComparison.OrdinalIgnoreCase).Trim();
        monster.SpeedDescription = speedText;
        var match = Regex.Match(speedText, @"(\d+)\s*ft");
        if (match.Success && int.TryParse(match.Groups[1].Value, out int speed))
        {
            monster.Speed = speed;
        }
    }

    private void ParseAbilityTableLine(string line, MonsterStatblock monster)
    {
        // Example: STR 23 +6 +6
        var match = Regex.Match(line, @"(STR|DEX|CON|INT|WIS|CHA)\s+(\d+)\s+([+-]?\d+)\s+([+-]?\d+)");
        if (match.Success)
        {
            var abbr = match.Groups[1].Value.ToUpper();
            int.TryParse(match.Groups[2].Value, out int score);
            int.TryParse(match.Groups[3].Value, out int mod);
            int.TryParse(match.Groups[4].Value, out int save);
            switch (abbr)
            {
                case "STR": monster.Strength = score; break;
                case "DEX": monster.Dexterity = score; break;
                case "CON": monster.Constitution = score; break;
                case "INT": monster.Intelligence = score; break;
                case "WIS": monster.Wisdom = score; break;
                case "CHA": monster.Charisma = score; break;
            }
            monster.AbilityMods[abbr] = mod;
            monster.AbilitySaves[abbr] = save;
        }
    }

    private void ParseSkills(string line, MonsterStatblock monster)
    {
        var content = line.Replace("Skills", "", StringComparison.OrdinalIgnoreCase).Trim();
        monster.Skills = content.Split(',').Select(s => s.Trim()).ToList();
    }

    private void ParseImmunities(string line, MonsterStatblock monster)
    {
        var content = line.Replace("Immunities", "", StringComparison.OrdinalIgnoreCase).Trim();
        monster.Immunities = content.Split(',').Select(s => s.Trim()).ToList();
    }

    private void ParseSenses(string line, MonsterStatblock monster)
    {
        var content = line.Replace("Senses", "", StringComparison.OrdinalIgnoreCase).Trim();
        monster.Senses = content.Split(',').Select(s => s.Trim()).ToList();
    }

    private void ParseLanguages(string line, MonsterStatblock monster)
    {
        var content = line.Replace("Languages", "", StringComparison.OrdinalIgnoreCase).Trim();
        monster.Languages = content.Split(',').Select(s => s.Trim()).ToList();
    }

    private void ParseCRandPB(string line, MonsterStatblock monster)
    {
        // Example: CR 14 (XP 11,500, or 13,000 in lair; PB +5)
        var match = Regex.Match(line, @"CR\s+([^(]+)\s*\(([^;]*);\s*PB\s*([+-]?\d+)\)", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            monster.ChallengeRating = match.Groups[1].Value.Trim();
            monster.ProficiencyBonus = int.TryParse(match.Groups[3].Value, out int pb) ? pb : 0;
            monster.ProficiencyBonusDescription = $"PB +{monster.ProficiencyBonus}";
        }
        else
        {
            // fallback: CR 14 (XP 11,500)
            var crMatch = Regex.Match(line, @"CR\s+([^(]+)", RegexOptions.IgnoreCase);
            if (crMatch.Success)
                monster.ChallengeRating = crMatch.Groups[1].Value.Trim();
        }
    }

    private void ParseLegendaryActionUses(string line, MonsterStatblock monster)
    {
        // Example: Legendary Action Uses: 3 (4 in Lair)
        var match = Regex.Match(line, @"Legendary Action Uses:\s*(\d+)(?:\s*\(([^)]+)\))?", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            monster.LegendaryActionUses = int.TryParse(match.Groups[1].Value, out int uses) ? uses : 0;
            monster.LegendaryActionUsesDescription = match.Groups[2].Value;
        }
    }
}