using Findx.Data;
using Findx.Mapping;
using Microsoft.AspNetCore.Mvc;

namespace Findx.AspNetCore.Mvc
{
    public abstract class CrudControllerBase<TModel, TDto, TCreateRequest, TUpdateRequest, TQuery, TKey> : ControllerBase 
        where TModel : class, new()
        where TCreateRequest : EntityBase, new()
        where TUpdateRequest : EntityBase, new()
        where TQuery : class, new()
    {
        private readonly IRepository<TModel> _repository;
        private readonly IMapper _mapper;

        /// <summary>
        /// 创建前操作
        /// </summary>
        /// <param name="dto">创建参数</param>
        protected virtual void CreateBefore(TCreateRequest dto) { }

        [HttpPost]
        public virtual CommonResult Create([FromBody] TCreateRequest request)
        {
            Check.NotNull(request, nameof(request));

            CreateBefore(request);

            var model = _mapper.MapTo<TModel>(request);

            _repository.Insert(model);

            return CommonResult.Success();
        }

        /// <summary>
        /// 修改前操作
        /// </summary>
        /// <param name="dto">修改参数</param>
        protected virtual void UpdateBefore(TUpdateRequest dto) { }

        [HttpPost]
        public virtual CommonResult Update(TUpdateRequest request)
        {
            Check.NotNull(request, nameof(request));

            var model = _mapper.MapTo<TModel>(request);

            UpdateBefore(request);

            _repository.Update(model);
            return CommonResult.Success();
        }
    }
}
