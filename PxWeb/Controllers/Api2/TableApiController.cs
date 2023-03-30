/*
 * PxApi
 *
 * No description provided (generated by Swagger Codegen https://github.com/swagger-api/swagger-codegen)
 *
 * OpenAPI spec version: 2.0
 * 
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using PxWeb.Attributes.Api2;
using PxWeb.Api2.Server.Models;
using PCAxis.Paxiom;
using Px.Abstractions.Interfaces;
using PxWeb.Helper.Api2;
using PxWeb.Mappers;
using Px.Search;
using System.Linq;
using Lucene.Net.Util;
using PxWeb.Code.Api2.Serialization;
using Microsoft.AspNetCore.Http;
using PxWeb.Config.Api2;
using System.Runtime.Serialization;

namespace PxWeb.Controllers.Api2
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    public class TableApiController : PxWeb.Api2.Server.Controllers.TableApiController
    {
        private readonly IDataSource _dataSource;
        private readonly ILanguageHelper _languageHelper;
        private readonly ITableMetadataResponseMapper _tableMetadataResponseMapper;
        private readonly ITablesResponseMapper _tablesResponseMapper;
        private readonly ISearchBackend _backend;
        private readonly IPxApiConfigurationService _pxApiConfigurationService;

        public TableApiController(IDataSource dataSource, ILanguageHelper languageHelper, ITableMetadataResponseMapper responseMapper, ISearchBackend backend, IPxApiConfigurationService pxApiConfigurationService, ITablesResponseMapper tablesResponseMapper)
        {
            _dataSource = dataSource;
            _languageHelper = languageHelper;
            _tableMetadataResponseMapper = responseMapper;
            _backend = backend;
            _pxApiConfigurationService = pxApiConfigurationService;
            _tablesResponseMapper = tablesResponseMapper;
        }


        public override IActionResult GetMetadataById([FromRoute(Name = "id"), Required] string id, [FromQuery(Name = "lang")] string? lang)
        {
            lang = _languageHelper.HandleLanguage(lang);
            IPXModelBuilder? builder = _dataSource.CreateBuilder(id, lang);


            if (builder != null)
            {
                try
                {
                    builder.BuildForSelection();
                    var model = builder.Model;

                    TableMetadataResponse tm = _tableMetadataResponseMapper.Map(model, id, lang);

                    return new ObjectResult(tm);
                }
                catch (Exception)
                {
                    return NotFound(NonExistentTable(id));
                }
            }
            else
            {
                return NotFound(NonExistentTable(id));
            }
        }

        private Problem NonExistentTable(string id)
        {
            Problem p = new Problem();
            p.Type = "Parameter error";
            p.Detail = "Non-existent table " + id;
            p.Status = 404;
            p.Title = "Non-existent table";
            return p;
        }

        public override IActionResult GetTableById([FromRoute(Name = "id"), Required] string id, [FromQuery(Name = "lang")] string? lang)
        {
            throw new NotImplementedException();
        }

        public override IActionResult GetTableCodeListById([FromRoute(Name = "id"), Required] string id, [FromQuery(Name = "lang")] string? lang)
        {
            throw new NotImplementedException();
        }



        private IDataSerializer GetSerializer(string outputFormat)
        {
            switch (outputFormat.ToLower())
            {
                case "xlsx":
                case "xlsx_doublecolumn":
                case "csv":
                case "csv_tab":
                case "csv_tabhead":
                case "csv_comma":
                case "csv_commahead":
                case "csv_space":
                case "csv_spacehead":
                case "csv_semicolon":
                case "csv_semicolonhead":
                case "csv2":
                case "csv3":
                case "json_stat":
                case "json_stat2":
                case "html5_table":
                case "relational_table":
                case "px":
                default:
                    return new PxDataSerializer();
            }

        }

        private Selection[] GetDefaultTable(PXModel model)
        {
            //TODO implement the correct algorithm

            var selections = new List<Selection>();

            foreach (var variable in model.Meta.Variables)
            {
                var selection = new Selection(variable.Code);
                //Takes the first 4 values for each variable if variable has less values it takes all of its values.
                var codes = variable.Values.Take(4).Select(value => value.Code).ToArray();
                selection.ValueCodes.AddRange(codes);
                selections.Add(selection);
            }

            return selections.ToArray();
        }

        public override IActionResult ListAllTables([FromQuery(Name = "lang")] string? lang, [FromQuery(Name = "query")] string? query, [FromQuery(Name = "pastDays")] int? pastDays, [FromQuery(Name = "includeDiscontinued")] bool? includeDiscontinued, [FromQuery(Name = "pageNumber")] int? pageNumber, [FromQuery(Name = "pageSize")] int? pageSize)
        {
            Searcher searcher = new Searcher(_dataSource, _backend);
            var op = _pxApiConfigurationService.GetConfiguration();
            
            lang = _languageHelper.HandleLanguage(lang);

            if (pageNumber == null || pageNumber <= 0)
                pageNumber = 1;

            if (pageSize == null || pageSize <= 0)
                pageSize = op.PageSize;

            var searchResult = searcher.Find(query, lang, pastDays, includeDiscontinued ?? false, pageSize.Value, pageNumber.Value);
            if (searchResult.Count() != 0)
            {
                return Ok(_tablesResponseMapper.Map(searchResult, lang, query));
            }
            return Ok();

        }

        public override IActionResult GetTableData([FromRoute(Name = "id"), Required] string id, [FromQuery(Name = "lang")] string? lang, [FromQuery(Name = "valuecodes")] Dictionary<string, List<string>>? valuecodes, [FromQuery(Name = "codelist")] Dictionary<string, string>? codelist, [FromQuery(Name = "outputvalues")] Dictionary<string, CodeListOutputValuesStyle>? outputvalues, [FromQuery(Name = "outputFormat")] string? outputFormat)
        {
            //TODO check that no selection paramaters is given
            lang = _languageHelper.HandleLanguage(lang);
            PXModel model;
            //if no parameters given
            var builder = _dataSource.CreateBuilder(id, lang);
            if (builder == null)
            {
                throw new Exception("Missing datasource");
            }

            builder.BuildForSelection();
            var selection = GetDefaultTable(builder.Model);

            builder.BuildForPresentation(selection);
            model = builder.Model;
            //else
            //    TODO create model from selection
            //    selection = GetSelectionFromQuery(...)

            //serialize output
            //TODO check if given in url param otherwise take the format from appsettings
            outputFormat = "px";
            var serializer = GetSerializer(outputFormat);
            serializer.Serialize(model, Response);

            return Ok();
        }

        public override IActionResult GetTableDataByPost([FromRoute(Name = "id"), Required] string id, [FromQuery(Name = "lang")] string? lang, [FromBody] VariablesSelection? variablesSelection)
        {
            throw new NotImplementedException();
        }
    }
}
