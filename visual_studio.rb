require 'rexml/document'
require 'fileutils'

module VisualStudio 
  class CsProj
    attr_reader :content
    def initialize(content)
      @content = content
      @xmldoc = REXML::Document.new(@content)
      @xmlns = {"x"=>"http://schemas.microsoft.com/developer/msbuild/2003"};
    end

    def files()
      files=[]
      ['Compile','Content','EmbeddedResource','None', 'Page'].each { |elementType|
          REXML::XPath.each(@xmldoc,"/x:Project/x:ItemGroup/x:#{elementType}", @xmlns) { |file|
            links = file.elements.select{ |el| el.name == 'Link' }
            depend_upon = file.elements.select { |el| el.name == 'DependentUpon'  }
            generator = file.elements.select { |el| el.name == 'Generator' }
            sub_type =  file.elements.select { |el| el.name == 'SubType' }
            files.push(FileReference.new({
              :file=>file.attributes['Include'], 
              :type=>elementType, 
              :link=> links.first,
              :dependent_upon=>depend_upon.first,
              :generator => generator.first,
              :sub_type => sub_type.first 
            }))
          }
      }
      return files
    end
    def add(ref)
      last = REXML::XPath.match(@xmldoc,"/x:Project/x:ItemGroup/", @xmlns).last
      el = last.add_element(ref.type,{'Include'=>ref.file})
      if ref.link
        el.add_element('Link').add_text(ref.link)
      end
      if ref.dependent_upon
        el.add_element('DependentUpon').add_text(ref.dependent_upon)
      end
      if ref.generator
        el.add_element('Generator').add_text(ref.generator)
      end
      if ref.sub_type
        el.add_element('SubType').add_text(ref.sub_type)
      end
    end
    def clear_links()
      ['Compile','Content','EmbeddedResource','None'].each { |elementType|
          REXML::XPath.each(@xmldoc,"/x:Project/x:ItemGroup/x:#{elementType}", @xmlns) { |file|
            links = file.elements.select{ |el| el.name == 'Link' }
            if (links.any?)
              file.parent.delete_element(file)
            end
          }
      }
    end
    def to_s
      return @xmldoc.to_s
    end
    def write output
      @xmldoc.write output
    end
  end

  class FileReference
    attr_reader :file, :downcase_and_path_replaced, :type, :link, :dependent_upon, :generator, :sub_type
    def initialize opts
      @file = opts[:file]
      @type = opts[:type]
      @link = opts[:link]
      @dependent_upon = opts[:dependent_upon]
      @sub_type = opts[:sub_type]
      @generator = opts[:generator]
      @downcase_and_path_replaced = @file.downcase.gsub(/\//,'\\')
    end
    def ==(other)
      other.downcase_and_path_replaced == @downcase_and_path_replaced
    end
    alias_method :eql?, :==
    def hash
      @downcase_and_path_replaced.hash
    end
    def to_opts
      return {:file=>@file,:type=>@type,:link=>@link,:dependent_upon=>@dependent_upon,:sub_type=>@sub_type,:generator=>@generator}
    end
    def to_s
      "#{@file} #{@type} #{@link}"
    end
  end

end
