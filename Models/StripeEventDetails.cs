using J2N.Collections.Generic;
using Newtonsoft.Json;

public class StripeEventDetails
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("object")]
    public string Object { get; set; }

    [JsonProperty("adaptive_pricing")]
    public AdaptivePricing AdaptivePricing { get; set; }

    [JsonProperty("after_expiration")]
    public object AfterExpiration { get; set; }

    [JsonProperty("allow_promotion_codes")]
    public object AllowPromotionCodes { get; set; }

    [JsonProperty("amount_subtotal")]
    public int AmountSubtotal { get; set; }

    [JsonProperty("amount_total")]
    public int AmountTotal { get; set; }

    [JsonProperty("automatic_tax")]
    public AutomaticTax AutomaticTax { get; set; }

    [JsonProperty("billing_address_collection")]
    public object BillingAddressCollection { get; set; }

    [JsonProperty("cancel_url")]
    public string CancelUrl { get; set; }

    [JsonProperty("client_reference_id")]
    public object ClientReferenceId { get; set; }

    [JsonProperty("client_secret")]
    public object ClientSecret { get; set; }

    [JsonProperty("consent")]
    public object Consent { get; set; }

    [JsonProperty("consent_collection")]
    public object ConsentCollection { get; set; }

    [JsonProperty("created")]
    public long Created { get; set; }

    [JsonProperty("currency")]
    public string Currency { get; set; }

    [JsonProperty("currency_conversion")]
    public object CurrencyConversion { get; set; }

    [JsonProperty("custom_fields")]
    public object[] CustomFields { get; set; }

    [JsonProperty("custom_text")]
    public CustomText CustomText { get; set; }

    [JsonProperty("customer")]
    public object Customer { get; set; }

    [JsonProperty("customer_creation")]
    public string CustomerCreation { get; set; }

    [JsonProperty("customer_details")]
    public CustomerDetails CustomerDetails { get; set; }

    [JsonProperty("customer_email")]
    public object CustomerEmail { get; set; }

    [JsonProperty("expires_at")]
    public long ExpiresAt { get; set; }

    [JsonProperty("invoice")]
    public object Invoice { get; set; }

    [JsonProperty("invoice_creation")]
    public InvoiceCreation InvoiceCreation { get; set; }

    [JsonProperty("livemode")]
    public bool Livemode { get; set; }

    [JsonProperty("locale")]
    public object Locale { get; set; }

    [JsonProperty("metadata")]
    public System.Collections.Generic.Dictionary<string, object> Metadata { get; set; }

    [JsonProperty("mode")]
    public string Mode { get; set; }

    [JsonProperty("payment_intent")]
    public string PaymentIntent { get; set; }

    [JsonProperty("payment_link")]
    public object PaymentLink { get; set; }

    [JsonProperty("payment_method_collection")]
    public string PaymentMethodCollection { get; set; }

    [JsonProperty("payment_method_configuration_details")]
    public object PaymentMethodConfigurationDetails { get; set; }

    [JsonProperty("payment_method_options")]
    public PaymentMethodOptions PaymentMethodOptions { get; set; }

    [JsonProperty("payment_method_types")]
    public string[] PaymentMethodTypes { get; set; }

    [JsonProperty("payment_status")]
    public string PaymentStatus { get; set; }

    [JsonProperty("phone_number_collection")]
    public PhoneNumberCollection PhoneNumberCollection { get; set; }

    [JsonProperty("recovered_from")]
    public object RecoveredFrom { get; set; }

    [JsonProperty("saved_payment_method_options")]
    public object SavedPaymentMethodOptions { get; set; }

    [JsonProperty("setup_intent")]
    public object SetupIntent { get; set; }

    [JsonProperty("shipping_address_collection")]
    public object ShippingAddressCollection { get; set; }

    [JsonProperty("shipping_options")]
    public object[] ShippingOptions { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("submit_type")]
    public object SubmitType { get; set; }

    [JsonProperty("subscription")]
    public object Subscription { get; set; }

    [JsonProperty("success_url")]
    public string SuccessUrl { get; set; }

    [JsonProperty("total_details")]
    public TotalDetails TotalDetails { get; set; }

    [JsonProperty("ui_mode")]
    public string UiMode { get; set; }

    [JsonProperty("url")]
    public object Url { get; set; }

    [JsonProperty("shipping_cost")]
    public object ShippingCost { get; set; }

    [JsonProperty("shipping_details")]
    public object ShippingDetails { get; set; }
}

public class AdaptivePricing
{
    [JsonProperty("enabled")]
    public bool Enabled { get; set; }
}

public class AutomaticTax
{
    [JsonProperty("enabled")]
    public bool Enabled { get; set; }

    [JsonProperty("liability")]
    public object Liability { get; set; }

    [JsonProperty("status")]
    public object Status { get; set; }
}

public class CustomText
{
    [JsonProperty("after_submit")]
    public object AfterSubmit { get; set; }

    [JsonProperty("shipping_address")]
    public object ShippingAddress { get; set; }

    [JsonProperty("submit")]
    public object Submit { get; set; }

    [JsonProperty("terms_of_service_acceptance")]
    public object TermsOfServiceAcceptance { get; set; }
}

public class CustomerDetails
{
    [JsonProperty("address")]
    public Address Address { get; set; }

    [JsonProperty("email")]
    public string Email { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("phone")]
    public object Phone { get; set; }

    [JsonProperty("tax_exempt")]
    public string TaxExempt { get; set; }

    [JsonProperty("tax_ids")]
    public object[] TaxIds { get; set; }
}

public class Address
{
    [JsonProperty("city")]
    public object City { get; set; }

    [JsonProperty("country")]
    public string Country { get; set; }

    [JsonProperty("line1")]
    public object Line1 { get; set; }

    [JsonProperty("line2")]
    public object Line2 { get; set; }

    [JsonProperty("postal_code")]
    public object PostalCode { get; set; }

    [JsonProperty("state")]
    public object State { get; set; }
}

public class InvoiceCreation
{
    [JsonProperty("enabled")]
    public bool Enabled { get; set; }

    [JsonProperty("invoice_data")]
    public InvoiceData InvoiceData { get; set; }
}

public class InvoiceData
{
    [JsonProperty("account_tax_ids")]
    public object AccountTaxIds { get; set; }

    [JsonProperty("custom_fields")]
    public object CustomFields { get; set; }

    [JsonProperty("description")]
    public object Description { get; set; }

    [JsonProperty("footer")]
    public object Footer { get; set; }

    [JsonProperty("issuer")]
    public object Issuer { get; set; }

    [JsonProperty("metadata")]
    public object Metadata { get; set; }

    [JsonProperty("rendering_options")]
    public object RenderingOptions { get; set; }
}

public class PaymentMethodOptions
{
    [JsonProperty("card")]
    public Card Card { get; set; }
}

public class Card
{
    [JsonProperty("request_three_d_secure")]
    public string RequestThreeDSecure { get; set; }
}

public class PhoneNumberCollection
{
    [JsonProperty("enabled")]
    public bool Enabled { get; set; }
}

public class TotalDetails
{
    [JsonProperty("amount_discount")]
    public int AmountDiscount { get; set; }

    [JsonProperty("amount_shipping")]
    public int AmountShipping { get; set; }

    [JsonProperty("amount_tax")]
    public int AmountTax { get; set; }
}
