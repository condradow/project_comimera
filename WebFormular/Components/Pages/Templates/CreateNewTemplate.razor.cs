using WebFormular.Classes;
using WebFormular.Classes.Elements;

namespace WebFormular.Components.Pages.Templates
{
    public partial class CreateNewTemplate
    {

        private DocumentBase Template { get; set; } = new()
        {
            Id = Guid.NewGuid(),
            CreatedOn = DateTime.Now,
            ModifiedOn = DateTime.Now,
            Title = string.Empty,
            Description = string.Empty
        };

        private ElementDefinition? SelectedDefinition { get; set; }


        private void AddElement()
        {
            if (SelectedDefinition is null)
                return;

            if (Activator.CreateInstance(SelectedDefinition.ModelType)
                is not ElementBase element)
                return;

            element.Document = Template;
            element.DocumentId = Template.Id;
            element.Title = SelectedDefinition.Name;

            Template.Elements.Add(element);

            SelectedDefinition = null;
        }


        private void RemoveElement(int index)
        {
            Template.Elements.RemoveAt(index);
        }


        private Type GetComponentType(ElementBase element)
        {
            var definition = ElementRegistry.Elements
                .FirstOrDefault(x => x.ModelType == element.GetType());

            if (definition is null)
                throw new InvalidOperationException(
                    $"Für {element.GetType().Name} wurde keine Komponente registriert.");

            return definition.ComponentType;
        }


        private Dictionary<string, object?> GetParameters(ElementBase element)
        {
            return new Dictionary<string, object?>
            {
                ["Model"] = element
            };
        }


        private async Task SaveTemplate()
        {
            // TODO:
            // Template über deinen Service / Repository speichern.

            Console.WriteLine($"Speichere: {Template.Title}");

            foreach (var element in Template.Elements)
            {
                Console.WriteLine(
                    $"{element.Title} - {element.GetType().Name}");
            }

            await Task.CompletedTask;
        }
    }
}