using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using SmartWinners.Helpers;

namespace SmartWinners.Models.Payment;

public class WithdrawForm
{
    public string CurrencyIso { get; set; }

    public string CountryIso { get; set; }

    public string CountryName { get; set; }

    public int Code { get; set; }

    public string CountryFlagUrl { get; set; }

    public bool AllowLocalLanguage { get; set; }

    public List<WithdrawFormField> MainFormFields { get; set; }

    public List<WithdrawFormField> SecondaryFormFields { get; set; }

    public static List<WithdrawForm> Init()
    {
        var doc = XDocument.Load($"{EnvironmentHelper.Environment.WebRootPath}/currency.xml");

        var root = doc.Root;

        var currencyElems = root.Elements().Where(x => x.Name.LocalName.Equals("currency"));

        var withdrawForms = new List<WithdrawForm>();

        foreach (var currencyElem in currencyElems)
        {
            var attributes = currencyElem.Attributes().ToList();

            var fieldElems = currencyElem.Elements().ToList();

            withdrawForms.Add(new WithdrawForm
            {
                CountryFlagUrl = attributes.First(x => x.Name.LocalName.Equals("icon")).Value,
                CountryIso = attributes.First(x => x.Name.LocalName.Equals("country")).Value,
                CountryName = attributes.First(x => x.Name.LocalName.Equals("fullCountry")).Value,
                CurrencyIso = attributes.First(x => x.Name.LocalName.Equals("name")).Value,
                Code = int.Parse(attributes.First(x => x.Name.LocalName.Equals("code")).Value),
                AllowLocalLanguage = fieldElems.First(x => x.Name.LocalName.Equals("AllowLocalLanguage")).Value == "1"
            });

            var lastForm = withdrawForms.Last();

            lastForm.MainFormFields = new List<WithdrawFormField>();

            fieldElems.Remove(fieldElems.First(x => x.Name.LocalName.Equals("AllowLocalLanguage")));

            if (fieldElems.Where(x => x.Name.LocalName.Equals("form")).ToList().Count == 2)
            {
                var forms = fieldElems.Where(x => x.Name.LocalName.Equals("form")).ToList();

                lastForm.SecondaryFormFields = new List<WithdrawFormField>();

                var count = 0;

                foreach (var formsFieldsElems in forms)
                {
                    count++;
                    
                    foreach (var fieldElem in formsFieldsElems.Elements())
                    {
                        var formFields = count < 2 ? lastForm.MainFormFields : lastForm.SecondaryFormFields;
                        
                        if (fieldElem.Name.LocalName.Equals("remark"))
                        {
                            formFields.Add(new WithdrawFormField
                            {
                                FieldType = FieldType.Remark,
                                Name = fieldElem.Value,
                                Title = fieldElem.Attributes().ToList().First(x => x.Name.LocalName.Equals("title")).Value
                            });
                            continue;
                        }

                        var fieldAttributes = fieldElem.Attributes().ToList();

                        formFields.Add(new WithdrawFormField
                        {
                            Name = fieldElem.Value,
                            Title = fieldAttributes.First(x => x.Name.LocalName.Equals("title")).Value,
                            WsName = fieldAttributes.First(x => x.Name.LocalName.Equals("wsname")).Value,
                            Target = fieldAttributes.First(x => x.Name.LocalName.Equals("target")).Value,
                        });

                        var lastField = formFields.Last();

                        var maxLenVal = fieldAttributes.First(x => x.Name.LocalName.Equals("maxlen")).Value;

                        if (int.TryParse(maxLenVal, out var maxLen))
                        {
                            lastField.FieldType = FieldType.Field;
                            lastField.MaxLength = maxLen;
                        }
                        else
                        {
                            lastField.FieldType = FieldType.DropDown;
                            lastField.DropDownOptions = [.. maxLenVal.Split(',')];
                        }
                    }
                }
                continue;
            }


            foreach (var fieldElem in fieldElems)
            {
                if (fieldElem.Name.LocalName.Equals("remark"))
                {
                    lastForm.MainFormFields.Add(new WithdrawFormField
                    {
                        FieldType = FieldType.Remark,
                        Name = fieldElem.Value,
                        Title = fieldElem.Attributes().ToList().First(x => x.Name.LocalName.Equals("title")).Value
                    });
                    continue;
                }

                var fieldAttributes = fieldElem.Attributes().ToList();

                lastForm.MainFormFields.Add(new WithdrawFormField
                {
                    Name = fieldElem.Value,
                    Title = fieldAttributes.First(x => x.Name.LocalName.Equals("title")).Value,
                    WsName = fieldAttributes.First(x => x.Name.LocalName.Equals("wsname")).Value,
                    Target = fieldAttributes.First(x => x.Name.LocalName.Equals("target")).Value,
                });

                var lastField = lastForm.MainFormFields.Last();

                var maxLenVal = fieldAttributes.First(x => x.Name.LocalName.Equals("maxlen")).Value;

                if (int.TryParse(maxLenVal, out var maxLen))
                {
                    lastField.FieldType = FieldType.Field;
                    lastField.MaxLength = maxLen;
                }
                else
                {
                    lastField.FieldType = FieldType.DropDown;
                    lastField.DropDownOptions = [.. maxLenVal.Split(',')];
                }
            }
        }

        return withdrawForms;
    }
}

public class WithdrawFormField
{
    public FieldType FieldType { get; set; }

    public int? MaxLength { get; set; } = null;

    public string WsName { get; set; }

    public string Target { get; set; }

    public string Title { get; set; }

    public List<string> DropDownOptions { get; set; }

    public string Name { get; set; }
}

public enum FieldType
{
    DropDown,
    Field,
    CheckBox,
    Remark
}