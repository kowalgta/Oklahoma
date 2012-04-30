using System;
using System.Collections.Generic;
using Machine.Fakes;
using Machine.Specifications;

namespace Oklahoma.Specs
{
    public abstract class SaleCycleContext : WithFakes
    {
        Establish context = () =>
        {
            SaleCycle.StateStorage = new InMemoryStateStorage();
            SaleCycle.SessionResolver = () => "123456qwe";
        };

        Cleanup after = () =>
            SaleCycle.StateStorage = new HttpContextStateStorage();

        protected static string formatAsPageVariable(string key, string value)
        {
            return String.Format("__sc[\"{0}\"]=\"{1}\";", key, value);
        }
    }

    [Subject(typeof(SaleCycle))]
    public class When_adding_a_page_scoped_variable : SaleCycleContext
    {
        It should_add_it_to_the_current_live_person_chat = () =>
            SaleCycle.Current.PageVariables["e"].ShouldEqual("john@gmail.com");

        Because of = () =>
            SaleCycle.AddPageVariable("e", "john@gmail.com");
    }

    [Subject(typeof(SaleCycle))]
    public class When_adding_a_page_scoped_variable_with_empty_name : SaleCycleContext
    {
        It should_throw_an_argument_null_exception = () =>
            Exception.ShouldBeOfType<ArgumentNullException>();

        Because of = () =>
            Exception = Catch.Exception(() => SaleCycle.AddPageVariable("", "john@gmail.com"));

        static Exception Exception;
    }

    public abstract class SaleCycleRenderContext : SaleCycleContext
    {
        Because of = () =>
            Exception = Catch.Exception(() => Result = SaleCycle.Render().ToString());

        protected static string Result;
        protected static Exception Exception;
    }

    [Behaviors]
    public class SaleCycleJavascriptRenderedBehavior
    {
        It should_include_sc_for_each_page_variable = () =>
        {
            foreach (var kvp in SaleCycle.Current.PageVariables)
                Result.ShouldContain(formatAsPageVariable(kvp.Key, kvp.Value));
        };

        It should_include_sc_for_client_id_page_variable = () =>
            Result.ShouldContain(formatAsPageVariable(SaleCycle.ClientIdVariable, SaleCycle.ClientId));

        It should_include_session_id = () =>
            Result.ShouldContain(formatAsPageVariable(SaleCycle.SessionIdVariable, SaleCycle.SessionResolver()));

        protected static string Result;

        protected static string formatAsPageVariable(string key, string value)
        {
            return String.Format("__sc[\"{0}\"]=\"{1}\";", key, value);
        }
    }

    [Subject(typeof(SaleCycle))]
    public class When_rendering_sale_cycle_javascript_and_cookie_behavior_is_set_to_default_value : SaleCycleRenderContext
    {
        Behaves_like<SaleCycleJavascriptRenderedBehavior> sale_cycle_javascript_rendered_behavior;

        It should_not_include_cookie_behavior_page_variable = () =>
            Result.ShouldNotContain(formatAsPageVariable(SaleCycle.CookieBehaviorVariable,
                ((short)SaleCycleCookieBehavior.CookiesAreEnabledAndSessionIdIsRequired).ToString()));

        Establish context = () =>
        {
            SaleCycle.ClientId = "1234567";
            SaleCycle.SetCartStatus(SaleCycleCartStatus.Checkout);
        };
    }

    [Subject(typeof(SaleCycle))]
    public class When_rendering_sale_cycle_javascript_and_client_id_was_not_set : SaleCycleRenderContext
    {
        It should_throw_an_exception = () =>
            Exception.ShouldBeOfType<ArgumentException>();

        Establish context = () =>
        {
            SaleCycle.ClientId = null;
            SaleCycle.SetCartStatus(SaleCycleCartStatus.Checkout);
        };
    }

    [Subject(typeof(SaleCycle))]
    public class When_rendering_sale_cycle_javascript_and_cart_status_was_not_set : SaleCycleRenderContext
    {
        It should_throw_an_exception = () =>
            Exception.ShouldBeOfType<ArgumentException>();

        Establish context = () =>
        {
            SaleCycle.ClientId = "1234567";
        };
    }

    [Subject(typeof(SaleCycle))]
    public class When_rendering_sale_cycle_javascript_and_cookie_behavior_is_default_and_session_id_is_not_defined : SaleCycleRenderContext
    {
        It should_throw_an_exception = () =>
            Exception.ShouldBeOfType<ArgumentException>();

        Establish context = () =>
        {
            SaleCycle.SessionResolver = null;
            SaleCycle.SetCartStatus(SaleCycleCartStatus.Checkout);
        };
    }

    [Subject(typeof(SaleCycle))]
    public class When_rendering_sale_cycle_javascript_and_cookie_behavior_is_different_from_default_and_session_id_is_not_defined : SaleCycleRenderContext
    {
        Behaves_like<SaleCycleJavascriptRenderedBehavior> sale_cycle_javascript_rendered_behavior;

        It should_include_cookie_behavior_page_variable = () =>
            Result.ShouldContain(formatAsPageVariable(SaleCycle.CookieBehaviorVariable,
                ((short)SaleCycleCookieBehavior.SaleCycleHandlesSessionManagement).ToString()));

        Establish context = () =>
        {
            SaleCycle.ClientId = "1234567";
            SaleCycle.CookieBehavior = SaleCycleCookieBehavior.SaleCycleHandlesSessionManagement;
            SaleCycle.SetCartStatus(SaleCycleCartStatus.Checkout);
        };
    }

    [Subject(typeof(SaleCycle))]
    public class When_rendering_sale_cycle_javascript_and_static_currency_was_defined : SaleCycleRenderContext
    {
        Behaves_like<SaleCycleJavascriptRenderedBehavior> sale_cycle_javascript_rendered_behavior;

        It should_include_currency_page_variable = () =>
            Result.ShouldContain(formatAsPageVariable(SaleCycle.CurrencyVariable, SaleCycle.Currency));

        Establish context = () =>
        {
            SaleCycle.ClientId = "1234567";
            SaleCycle.Currency = "GBP";
            SaleCycle.SetCartStatus(SaleCycleCartStatus.Checkout);
        };
    }

    [Subject(typeof(SaleCycle))]
    public class When_setting_customer_email : SaleCycleContext
    {
        It should_set_customer_email = () =>
            SaleCycle.Current.PageVariables[SaleCycle.CustomerEmailVariable].ShouldEqual(Email);

        Because of = () =>
            SaleCycle.SetCustomerEmail(Email);

        protected static string Email = "harry@hogwart.com";
    }

    [Subject(typeof(SaleCycle))]
    public class When_setting_customer_phone_number : SaleCycleContext
    {
        It should_set_customer_phone_number = () =>
            SaleCycle.Current.PageVariables[SaleCycle.CustomerPhoneNumberVariable].ShouldEqual(PhoneNumber);

        Because of = () =>
            SaleCycle.SetCustomerPhoneNumber(PhoneNumber);

        protected static string PhoneNumber = "02010101010";
    }

    [Subject(typeof(SaleCycle))]
    public class When_setting_customer_name_and_first_name_is_not_null : SaleCycleContext
    {
        It should_set_customer_name = () =>
            SaleCycle.Current.PageVariables[SaleCycle.CustomerNameVariable]
                .ShouldEqual(String.Format("{0}|{1}|{2}", FirstName, LastName, Title));

        Because of = () =>
            SaleCycle.SetCustomerName(FirstName, LastName, Title);
        
        protected static string FirstName = "Harry";
        protected static string LastName = "Potter";
        protected static string Title = "Mr";
    }

    [Subject(typeof(SaleCycle))]
    public class When_setting_customer_name_and_first_name_is_null : SaleCycleContext
    {
        It should_not_set_customer_name = () =>
            SaleCycle.Current.PageVariables.ContainsKey(SaleCycle.CustomerNameVariable).ShouldBeFalse();

        Because of = () =>
            SaleCycle.SetCustomerName(FirstName, LastName, Title);

        protected static string FirstName = null;
        protected static string LastName = "Potter";
        protected static string Title = "Mr";
    }

    [Subject(typeof(SaleCycle))]
    public class When_setting_cart_status : SaleCycleContext
    {
        It should_set_cart_status = () =>
            SaleCycle.Current.PageVariables[SaleCycle.CartStatusVariable].ShouldEqual(((short)SaleCycleCartStatus.Checkout).ToString());

        Because of = () =>
            SaleCycle.SetCartStatus(SaleCycleCartStatus.Checkout);
    }

    [Subject(typeof(SaleCycle))]
    public class When_setting_total_value : SaleCycleContext
    {
        It should_set_total_value_properly_formatted = () =>
            SaleCycle.Current.PageVariables[SaleCycle.TotalValueVariable].ShouldEqual("10.20");

        Because of = () =>
            SaleCycle.SetTotalValue(10.2m);
    }

    [Subject(typeof(SaleCycle))]
    public class When_setting_custom_field_one : SaleCycleContext
    {
        It should_set_custom_field_one_as_pipelined_input_values = () =>
            SaleCycle.Current
                .PageVariables[SaleCycle.CustomFieldOneVariable]
                .ShouldEqual("test string that should be pipelined | with javascript \\\"encoding\\\" | withouth extra characters");

        Because of = () =>
            SaleCycle.SetCustomFieldOne(new List<string>
                    {
                        "test string that should be pipelined ",
                        " with javascript \"encoding\" ",
                        " withouth extra|characters"
                    });
    }

    [Subject(typeof(SaleCycle))]
    public class When_setting_custom_field_one_and_there_is_only_one_input_string : SaleCycleContext
    {
        It should_set_custom_field_one_as_pipelined_input_values = () =>
            SaleCycle.Current.PageVariables[SaleCycle.CustomFieldOneVariable].ShouldEqual("test string");

        Because of = () =>
            SaleCycle.SetCustomFieldOne(new[] { "test|string" });
    }

    [Subject(typeof(SaleCycle))]
    public class When_setting_custom_field_two : SaleCycleContext
    {
        It should_set_custom_field_one_as_pipelined_input_values = () =>
            SaleCycle.Current
                .PageVariables[SaleCycle.CustomFieldOneVariable]
                .ShouldEqual("test string that should be pipelined | with javascript \\\"encoding\\\" | withouth extra characters");

        Because of = () =>
            SaleCycle.SetCustomFieldOne(new[]
                                        {
                                            "test string that should be pipelined ",
                                            " with javascript \"encoding\" ",
                                            " withouth extra|characters"
                                        });
    }

    [Subject(typeof(SaleCycle))]
    public class When_setting_web_page_name : SaleCycleContext
    {
        It should_set_web_page_name = () =>
            SaleCycle.Current.PageVariables[SaleCycle.PageNameVariable].ShouldEqual("Basket | eSpares");

        Because of = () =>
            SaleCycle.SetPageName("Basket | eSpares");
    }

    [Subject(typeof(SaleCycle))]
    public class When_no_cart_items_were_added : SaleCycleContext
    {
        It should_not_include_any_of_cart_item_variables = () =>
        {
            SaleCycle.Current.PageVariables.ContainsKey(SaleCycle.CartItemIdsVariable).ShouldBeFalse();
            SaleCycle.Current.PageVariables.ContainsKey(SaleCycle.CartItemNamesVariable).ShouldBeFalse();
            SaleCycle.Current.PageVariables.ContainsKey(SaleCycle.CartItemValuesVariable).ShouldBeFalse();
            SaleCycle.Current.PageVariables.ContainsKey(SaleCycle.CartItemQuantitiesVariable).ShouldBeFalse();
            SaleCycle.Current.PageVariables.ContainsKey(SaleCycle.CartItemImageUrlsVariable).ShouldBeFalse();
        };
    }

    [Subject(typeof(SaleCycle))]
    public class When_one_cart_item_was_added : SaleCycleContext
    {
        It should_include_all_item_variables = () =>
        {
            SaleCycle.Current.PageVariables[SaleCycle.CartItemIdsVariable].ShouldEqual("es123");
            SaleCycle.Current.PageVariables[SaleCycle.CartItemNamesVariable].ShouldEqual("name");
            SaleCycle.Current.PageVariables[SaleCycle.CartItemValuesVariable].ShouldEqual("12.00");
            SaleCycle.Current.PageVariables[SaleCycle.CartItemQuantitiesVariable].ShouldEqual("5");
            SaleCycle.Current.PageVariables[SaleCycle.CartItemImageUrlsVariable].ShouldEqual("http://espares.co.uk/es123/image.jpg");
        };

        Because of = () =>
            SaleCycle.AddCartItem("es123", "name", 12.0m, 5, "http://espares.co.uk/es123/image.jpg");
    }

    [Subject(typeof(SaleCycle))]
    public class When_many_cart_items_were_added : SaleCycleContext
    {
        It should_include_all_item_variables_pipe_delimited = () =>
        {
            SaleCycle.Current.PageVariables[SaleCycle.CartItemIdsVariable].ShouldEqual("es123|es666");
            SaleCycle.Current.PageVariables[SaleCycle.CartItemNamesVariable].ShouldEqual("name|name2");
            SaleCycle.Current.PageVariables[SaleCycle.CartItemValuesVariable].ShouldEqual("12.00|5.00");
            SaleCycle.Current.PageVariables[SaleCycle.CartItemQuantitiesVariable].ShouldEqual("5|1");
            SaleCycle.Current.PageVariables[SaleCycle.CartItemImageUrlsVariable].ShouldEqual("http://espares.co.uk/es123/image.jpg|http://espares.co.uk/es666/image.jpg");
        };

        Because of = () =>
        {
            SaleCycle.AddCartItem("es123", "name", 12.0m, 5, "http://espares.co.uk/es123/image.jpg");
            SaleCycle.AddCartItem("es666", "name2", 5.0m, 1, "http://espares.co.uk/es666/image.jpg");
        };
    }

    [Subject(typeof(SaleCycle))]
    public class When_many_cart_items_were_added_but_image_urls_were_null : SaleCycleContext
    {
        It should_include_all_item_variables_without_image_variables_pipe_delimited = () =>
        {
            SaleCycle.Current.PageVariables[SaleCycle.CartItemIdsVariable].ShouldEqual("es123|es666");
            SaleCycle.Current.PageVariables[SaleCycle.CartItemNamesVariable].ShouldEqual("name|name2");
            SaleCycle.Current.PageVariables[SaleCycle.CartItemValuesVariable].ShouldEqual("12.00|5.00");
            SaleCycle.Current.PageVariables[SaleCycle.CartItemQuantitiesVariable].ShouldEqual("5|1");            
        };

        It should_not_include_image_variables = () =>
            SaleCycle.Current.PageVariables.ContainsKey(SaleCycle.CartItemImageUrlsVariable).ShouldNotBeNull();

        Because of = () =>
        {
            SaleCycle.AddCartItem("es123", "name", 12.0m, 5, null);
            SaleCycle.AddCartItem("es666", "name2", 5.0m, 1, null);
        };
    }
}